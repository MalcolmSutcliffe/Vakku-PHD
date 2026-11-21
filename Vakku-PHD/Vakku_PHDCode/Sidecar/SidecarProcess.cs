using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;

namespace Vakku_PHD.Vakku_PHDCode.Sidecar;

/// <summary>Starts the bundled sidecar process on a free localhost port and waits until it answers. The process is stopped when the game exits.</summary>
public sealed class SidecarProcess : IDisposable
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private Process? _process;

    public int Port { get; private set; }
    public string BaseUrl => $"http://127.0.0.1:{Port}";

    /// <summary>The folder the sidecar was shipped in: next to the mod's DLL.</summary>
    public static string SidecarDir => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "sidecar");

    private static string LauncherName => OperatingSystem.IsWindows() ? "run.cmd" : "run";

    private static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    /// <summary>Returns the sidecar's /hello reply, or throws with the reason it could not start.</summary>
    public async Task<string> StartAsync(TimeSpan timeout)
    {
        string launcher = Path.Combine(SidecarDir, LauncherName);
        if (!File.Exists(launcher))
            throw new FileNotFoundException($"sidecar launcher not found: {launcher}");

        if (!OperatingSystem.IsWindows())
        {
            // A Workshop download may not keep the executable bit.
            var mode = File.GetUnixFileMode(launcher);
            MainFile.Logger.Info($"sidecar launcher mode before: {mode}");
            File.SetUnixFileMode(launcher, mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }

        Port = FreePort();
        var info = new ProcessStartInfo(launcher, $"--port {Port} --parent-pid {Environment.ProcessId}")
        {
            WorkingDirectory = SidecarDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        // Steam's 32-bit overlay is preloaded into every process the game starts; the sidecar is 64-bit and only gets warnings from it.
        info.Environment.Remove("LD_PRELOAD");
        _process = Process.Start(info) ?? throw new InvalidOperationException("Process.Start returned null");
        _process.OutputDataReceived += (_, e) => { if (e.Data != null) MainFile.Logger.Info($"[sidecar] {e.Data}"); };
        _process.ErrorDataReceived += (_, e) => { if (e.Data != null) MainFile.Logger.Warn($"[sidecar] {e.Data}"); };
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Dispose();

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (_process.HasExited)
                throw new InvalidOperationException($"sidecar exited early with code {_process.ExitCode}");
            try
            {
                return await _http.GetStringAsync($"{BaseUrl}/hello");
            }
            catch (HttpRequestException) { await Task.Delay(100); }
            catch (TaskCanceledException) { await Task.Delay(100); }
        }
        throw new TimeoutException($"sidecar did not answer on port {Port} within {timeout.TotalSeconds:0.#}s");
    }

    public void Dispose()
    {
        try
        {
            if (_process is { HasExited: false })
                _process.Kill(entireProcessTree: true);
        }
        catch { /* already gone */ }
        _http.Dispose();
    }
}
