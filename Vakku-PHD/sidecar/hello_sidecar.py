"""Stand-in for the real sidecar (engine + MCTS), to prove the mod can launch a bundled process and talk to it over localhost.
Usage: hello_sidecar.py --port N --parent-pid P. Exits by itself when its parent (the game) is gone."""
import argparse, json, os, sys, threading, time
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer

VERSION = "0.0.1"

class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == "/hello":
            body = json.dumps({"sidecar": "hello", "version": VERSION, "pid": os.getpid(), "python": sys.version.split()[0]}).encode()
            self.send_response(200); self.send_header("Content-Type", "application/json"); self.send_header("Content-Length", str(len(body))); self.end_headers()
            self.wfile.write(body)
        else:
            self.send_response(404); self.end_headers()
    def log_message(self, *args): pass

def watch_parent(pid):
    while True:
        try:
            os.kill(pid, 0)
        except OSError:
            os._exit(0)
        time.sleep(2)

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--port", type=int, required=True)
    p.add_argument("--parent-pid", type=int, default=0)
    a = p.parse_args()
    if a.parent_pid:
        threading.Thread(target=watch_parent, args=(a.parent_pid,), daemon=True).start()
    ThreadingHTTPServer(("127.0.0.1", a.port), Handler).serve_forever()

if __name__ == "__main__":
    main()
