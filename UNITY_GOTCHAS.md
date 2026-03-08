# Unity Project — Problems, Mistakes & Fixes Log

A practical reference of real issues encountered while building this project.
Useful for any AI assistant or developer picking up this codebase.

---

## 1. Sprite FileID — Wrong ID Causes Invisible/White Images

**Problem:** After setting up button and background images in a `.unity` scene file to use sprites from the `Assets/Transparent UI/` folder, all images appeared as flat white or flat color rectangles in the Editor — no actual texture appeared.

**Root cause:** The sprites (`Candy Background.png`, `Glittery Background.png`) use `spriteMode: 2` (Multiple/Sliced mode), which means each sprite is a **sub-asset** with its own unique `internalID`. When you write a scene file manually (e.g. via MCP tool or script), using the generic fallback fileID `21300000` only works for **Single** mode sprites. For Multiple-mode sprites, you must use the exact `internalID` from the `.meta` file.

**How to find the correct fileID:**
Open `Assets/Transparent UI/SpriteNameHere.png.meta` and look inside `spriteSheet.sprites[0].internalID`:
```yaml
spriteSheet:
  sprites:
  - name: Candy Background_0
    internalID: -1148866542617479634   # ← THIS is the correct fileID
```

**Correct references:**
| Sprite | fileID |
|---|---|
| `Candy Background.png` | `-1148866542617479634` |
| `Glittery Background.png` | `495631689734099677` |

**Correct format in scene file:**
```yaml
# WRONG (generic, only works for Single sprites):
m_Sprite: {fileID: 21300000, guid: e035ad5e75ad7ff469116582c67360c5, type: 3}

# CORRECT (use internalID from .meta):
m_Sprite: {fileID: -1148866542617479634, guid: e035ad5e75ad7ff469116582c67360c5, type: 3}
```

**Rule of thumb:** Always check `spriteMode` in the `.meta` file. If it's `2`, look up the `internalID`. If it's `1`, `21300000` works fine.

---

## 2. MCP Tool Created UI Elements With Wrong LocalScale (0.208 Instead of 1.0)

**Problem:** Buttons appeared tiny — roughly 20% of their intended size — even though `m_SizeDelta` was set to 400×70.

**Root cause:** The Unity MCP tool that created GameObjects set `m_LocalScale: {x: 0.2083333, y: 0.2083333, z: 0.2083333}` on UI RectTransform objects. This is likely a scaling artefact from how the tool converts world-space coordinates. Normal Unity UI objects should always have scale `{x: 1, y: 1, z: 1}`.

**Fix:** Do a search-and-replace in the `.unity` file for all UI elements:
```
WRONG:  m_LocalScale: {x: 0.2083333, y: 0.2083333, z: 0.2083333}
FIXED:  m_LocalScale: {x: 1, y: 1, z: 1}
```

**Rule of thumb:** After any MCP-created scene, check `m_LocalScale` on all UI RectTransforms. They should all be `1, 1, 1`.

---

## 3. Canvas Root RectTransform Has Scale (0, 0, 0)

**Problem/Note:** The root Canvas RectTransform (the one that has `m_SizeDelta: {x: 0, y: 0}` and anchors at 0,0 → 0,0) has `m_LocalScale: {x: 0, y: 0, z: 0}`. This looks alarming but it's **correct** — Unity sets the Canvas root to scale 0 intentionally and calculates display size from the CanvasScaler instead. Do **not** change this to 1.

---

## 4. Double `OnClick` Listener on a Button Causes Both Scenes to Load

**Problem:** Clicking the "Play" button in Main Menu immediately launched the game scene (`Candy Catcher`), skipping Level Select entirely.

**Root cause:** The Play button in `Assets/Scenes/Main Menu.unity` had **two** `OnClick` entries:
1. `MenuManager.Play()` — loads `"Level Select"` (correct, added later)
2. `SceneLoader.LoadGame()` — loads `"Candy Catcher"` directly (leftover from original setup)

Both fired simultaneously. Since `SceneLoader` loaded the game scene, Level Select was never seen.

**Fix:** Remove the `SceneLoader.LoadGame` persistent call entry from the button's `m_OnClick.m_PersistentCalls.m_Calls` array in `Main Menu.unity`. Keep only the `MenuManager.Play` entry.

**Rule of thumb:** When a button triggers the wrong scene, open the `.unity` file and search for `m_Calls` near the button's name to see all registered listeners.

---

## 5. MCP Transport — SSE vs stdio

**Problem:** MCP server for Unity kept showing `ENOENT` errors and failing to connect.

**Root cause:** The initial VS Code `mcp.json` had `"command": "other"` which attempts stdio transport and tries to launch a local executable. The CoplayDev Unity MCP server runs as an HTTP server inside the Unity Editor, not as a spawned process.

**Fix:** Use `type: sse` with the URL the server advertises:
```json
{
  "servers": {
    "unity-mcp": {
      "type": "sse",
      "url": "http://127.0.0.1:8080/mcp"
    }
  }
}
```

**How to start the MCP server:** In Unity Editor → top menu → `MCP for Unity > Start Server` (or it starts automatically when you open the project if the package is installed).

---

## 6. Build Settings Scene GUID Placeholder

**Problem:** After creating the `Level Select.unity` scene file, the `ProjectSettings/EditorBuildSettings.asset` had a placeholder `{guid: 000000000000000000000000000000000}` for the Level Select entry, causing Unity to show "Scene could not be found" at runtime.

**Fix:** Find the real GUID from `Assets/Scenes/Level Select.unity.meta` and update `EditorBuildSettings.asset`:
```yaml
# Wrong:
- enabled: 1
  path: Assets/Scenes/Level Select.unity
  guid: 00000000000000000000000000000000

# Fixed (real GUID from .meta file):
- enabled: 1
  path: Assets/Scenes/Level Select.unity
  guid: 6496dbe094d0c414e859b64667c9f7ea
```

---

## 7. Git — 28,000+ Phantom Changed Files

**Problem:** Cloning or reopening the project showed ~28,360 file changes in git, making source control unusable.

**Root causes (two separate issues):**
1. No `.gitignore` — Unity auto-generated folders (`Library/`, `Temp/`, `Logs/`, `.vs/`) were tracked.
2. `core.autocrlf=true` — Windows git was converting all line endings, causing every file to show as changed on re-checkout.

**Fix:**
```bash
# 1. Create a proper Unity .gitignore (or add one from gitignore.io for "Unity")
# 2. Remove already-tracked generated files:
git rm -r --cached Library/ Temp/ Logs/ .vs/ *.csproj *.sln
# 3. Fix line endings:
git config core.autocrlf false
# 4. Commit the .gitignore
git add .gitignore
git commit -m "Add Unity gitignore, remove tracked generated files"
```

---

## 8. CanvasScaler `MatchWidthOrHeight` for Portrait Mobile

**Problem:** Level Select layout looked wrong on portrait phone screens — huge gaps between buttons, elements bunched at the top.

**Root cause:** The CanvasScaler had `m_MatchWidthOrHeight: 0` (width-only scaling). On a tall portrait screen, width matches fine but height is stretched far beyond the reference, making vertical gaps appear enormous.

**Fix for portrait-primary games:**
```yaml
m_MatchWidthOrHeight: 0.5   # Balance both axes
```
Also make sure the reference resolution matches the Main Menu scene (`800×700`) so both scenes scale consistently.

---

## 9. LevelSelectManager Auto-Wire Approach (No Inspector Wiring Needed)

**Why this matters:** When creating a scene via code/MCP tools, you can't easily set up Inspector references (the `OnClick` component callbacks require serialized object references). Instead, `LevelSelectManager.cs` uses `Awake()` to find and wire buttons by name at runtime:

```csharp
void Awake() {
    var buttons = Object.FindObjectsByType<Button>(FindObjectsSortMode.None);
    foreach (var btn in buttons) {
        switch (btn.name) {
            case "Level1Button": btn.onClick.AddListener(SelectLevel1); break;
            case "Level2Button": btn.onClick.AddListener(SelectLevel2); break;
            case "Level3Button": btn.onClick.AddListener(SelectLevel3); break;
            case "BackButton":   btn.onClick.AddListener(BackToMenu);   break;
        }
    }
}
```

This avoids all serialized-reference issues. **Button GameObjects must be named exactly** `Level1Button`, `Level2Button`, `Level3Button`, `BackButton` for auto-wire to work.

---

## 10. Scene Load Order — Always Open the First Scene Before Playing

**Problem:** Pressing Play in the Unity Editor launched directly into `Candy Catcher` game scene even after adding Level Select.

**Root cause:** Unity Play mode runs whatever scene is **currently open**, not necessarily the first scene in Build Settings.

**Correct test flow:**
1. Double-click `Assets/Scenes/Main Menu.unity` in the Project window (make sure it's the active scene in the Editor)
2. Press ▶ Play
3. Click the in-game "Play" button → Level Select → pick a level → Candy Catcher

---

## Scene & Script Inventory

| File | Status | Notes |
|---|---|---|
| `Assets/Scripts/LevelConfig.cs` | Created | Difficulty presets + `LevelSelection` static bridge |
| `Assets/Scripts/LevelSelectManager.cs` | Created | Button auto-wire via `FindObjectsByType<Button>()` |
| `Assets/Scripts/GameManager.cs` | Modified | Reads `LevelSelection.Selected` on Start |
| `Assets/Scripts/CandySpawner.cs` | Modified | Added `trickChance` field + `ApplyConfig()` |
| `Assets/Scripts/MenuManager.cs` | Modified | `Play()` now loads `"Level Select"` |
| `Assets/Scenes/Level Select.unity` | Created | Portrait layout, glittery buttons, candy background |
| `Assets/Scenes/Main Menu.unity` | Fixed | Removed duplicate `SceneLoader.LoadGame` OnClick |
| `ProjectSettings/EditorBuildSettings.asset` | Modified | Real GUID for Level Select, scene order 0/1/2 |
| `ProjectSettings/ProjectSettings.asset` | Modified | Portrait-only (landscape autorotate disabled) |
