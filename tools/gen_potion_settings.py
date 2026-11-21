"""Writes one price slider per potion into VakkuConfig.cs (between the potion-prices markers) and their labels into settings_ui.json.
Run again when the engine gains a potion:  python3 tools/gen_potion_settings.py [path to the engine's scripts folder]"""
import json, re, sys
from pathlib import Path

root = Path(__file__).resolve().parent.parent
scripts = Path(sys.argv[1] if len(sys.argv) > 1 else "/home/user/Documents/Malcolm_Stuff/Projects/SlayTheSpireII-Agent-autorunner/scripts")
sys.path.insert(0, str(scripts))
from sts2_lite.potion import PotionId            # noqa: E402
from agents.sts2_MCTS.params import RewardParams  # noqa: E402

prices = RewardParams()
pascal = lambda name: "".join(w.capitalize() for w in name.split("_"))
title = lambda name: " ".join(w.capitalize() for w in name.split("_")).replace("Of", "of").replace("The", "the").replace("In A", "in a")

lines, entries, loc = [], [], {}
for i, potion in enumerate(PotionId):
    value = prices.potion_bonus.get(potion, prices.default_potion_bonus)
    prop = "PotionPrice" + pascal(potion.name)
    section = '    [ConfigSection("PotionPrices", CollapsedByDefault = true)]\n' if i == 0 else ""
    lines.append(f'{section}    [ConfigSlider(0, 60, 1, Format = "{{0}} HP")]\n    public static double {prop} {{ get; set; }} = {value};\n')
    entries.append(f'["{potion.name}"] = {prop},')
    key = "VAKKU_PHD-" + re.sub(r"(?<!^)(?=[A-Z])", "_", prop).upper()
    loc[key + ".title"] = title(potion.name)
    loc[key + ".hover.desc"] = f"HP {title(potion.name)} is worth when 'Only in danger' is off."
block = "".join(lines) + "\n    /// <summary>The price of every potion, by game id.</summary>\n    public static Dictionary<string, double> PotionPriceTable() => new()\n    {\n        " + "\n        ".join(entries) + "\n    };\n"

cfg = root / "Vakku-PHD/Vakku_PHDCode/Search/VakkuConfig.cs"
text = cfg.read_text()
text = re.sub(r"(// <potion-prices>\n).*?(    // </potion-prices>)", lambda m: m.group(1) + block + m.group(2), text, flags=re.S)
cfg.write_text(text)

path = root / "Vakku-PHD/Vakku_PHD/localization/eng/settings_ui.json"
data = {k: v for k, v in json.loads(path.read_text()).items() if "-POTION_PRICE_" not in k}
data["VAKKU_PHD-POTION_PRICES.title"] = "Potion prices"
data.update(loc)
path.write_text(json.dumps(data, indent=2))
print(len(lines), "potions")
