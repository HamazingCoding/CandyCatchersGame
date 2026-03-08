# MCP for Unity — Setup & Capabilities

## Every Time You Work on the Game

### Step 1 — Start the Server in Unity
1. Open Unity with the CandyCatchersGame project
2. Go to **Window > MCP for Unity**
3. Click **Start Server**
4. Wait for the log to show:
   ```
   INFO: Uvicorn running on http://127.0.0.1:8080
   Plugin registered: CandyCatchersGame
   Registered 23 tools
   ```

### Step 2 — Connect in VS Code
1. Open VS Code
2. Open GitHub Copilot chat
3. Switch to **Agent mode** (dropdown next to the chat input)
4. You should see **35 tools** available — MCP is connected
5. If it shows an error, do `Ctrl+Shift+P` → **Reload Window** and try again

> **Note:** The server must be started in Unity BEFORE opening VS Code chat. If you start Unity after, reload the VS Code window.

---

## What Copilot Can Do (via MCP)

With MCP connected, Copilot can directly:

- **Read & modify scene hierarchy** — inspect GameObjects, components, properties
- **Create GameObjects** — add new objects to the scene with correct settings
- **Add & configure components** — attach scripts, colliders, rigidbodies, etc.
- **Edit script files** — write and modify C# scripts directly
- **Assign Inspector references** — wire up script fields (e.g. drag UIManager into GameManager)
- **Read console logs** — see Unity errors and warnings in real time
- **Create & manage prefabs** — make reusable GameObjects
- **Manage assets** — search, read, and reference assets in the project
- **Control play mode** — start/stop play mode
- **Run tests** — execute Unity Test Runner tests
- **Manage UI** — create and configure Canvas, Buttons, Text elements
- **Manage animations** — read/write animation controllers and clips

---

## What Still Requires Manual Work in Unity Editor

Some things are faster or safer to do by hand:

| Task | Why manual is better |
|---|---|
| Positioning objects visually in the scene | Dragging in the Scene view is faster than typing coordinates |
| Adjusting collider shapes/sizes | Visual handles make this much easier |
| Setting up Camera framing | Visual preview needed |
| Importing new assets (sprites, audio, fonts) | Drag & drop into Project window |
| Creating new scenes from scratch | File → New Scene, then build layout |
| Animator state machine layout | Visual graph editor |
| Build & export to platform | Build Settings → Build |
| Testing actual gameplay feel | Play mode, controller in hand |

---

## Config Reference

**mcp.json location:** `%APPDATA%\Code\User\mcp.json`

```json
{
    "servers": {
        "coplaydev/unity-mcp": {
            "type": "sse",
            "url": "http://127.0.0.1:8080/mcp"
        }
    }
}
```

**Unity MCP package:** installed via Package Manager (git URL)
```
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
```
