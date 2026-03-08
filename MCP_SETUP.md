# MCP for Unity — Setup & Capabilities

## One-Time Config Setup

### Step 1 — Install the Unity Package
1. Open Unity with the CandyCatchersGame project
2. Go to **Window > Package Manager**
3. Click **+** → **Add package from git URL...**
4. Paste: `https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main`
5. Click **Add** and wait for import

### Step 2 — Configure Both Clients in Unity
1. Go to **Window > MCP for Unity**
2. Click **Start Server**
3. In the **Client** dropdown select **VSCode GitHub Copilot** → click **Configure**
4. Click **Configure All Detected Clients** to also add the second server entry
5. Look for 🟢 **Configured** status

### Step 3 — Set up mcp.json in VS Code
The file lives at `%APPDATA%\Code\User\mcp.json`. It should contain **both** entries:

```json
{
  "servers": {
    "coplaydev/unity-mcp": {
      "type": "sse",
      "url": "http://127.0.0.1:8080/mcp"
    },
    "unityMCP": {
      "type": "http",
      "url": "http://127.0.0.1:8080/mcp"
    }
  }
}
```

> If the file has `"command": "other"` or `"command": "uv"` instead, replace it with the above — those were the broken defaults.

---

## Every Time You Work on the Game

### Step 1 — Start the Server in Unity
1. Open Unity with the CandyCatchersGame project
2. Go to **Window > MCP for Unity**
3. Click **Start Server**
4. Wait for the log to show:
   ```
   INFO: Uvicorn running on http://127.0.0.1:8080
   Plugin registered: CandyCatchersGame
   Registered 23+ tools
   ```

### Step 2 — Connect in VS Code
1. Open VS Code
2. Open GitHub Copilot chat
3. Switch to **Agent mode** (dropdown next to the chat input)
4. You should see **35+ tools** available — MCP is connected
5. If it shows an error, do `Ctrl+Shift+P` → **Reload Window** and try again

> **Note:** The server must be started in Unity BEFORE opening VS Code chat. If you start Unity after, reload the VS Code window.

---

## What Copilot Can Do (via MCP)

With both MCP entries connected, Copilot can directly:

**Scenes & GameObjects**
- Read & modify scene hierarchy — inspect GameObjects, components, properties
- Create new scenes programmatically
- Create GameObjects with correct settings
- Add & configure components — scripts, colliders, rigidbodies, etc.
- Assign Inspector references — wire up script fields without manual dragging

**UI**
- Create Canvas, Buttons, Text, Images, Panels from scratch
- Configure layout, anchors, and colors
- Wire Button `OnClick` events to script methods

**Scripts**
- Write and modify C# scripts directly
- Validate scripts for compile errors before saving
- Execute batch script operations

**Assets & Prefabs**
- Create and save prefabs
- Search, read, and reference assets in the project
- Generate textures procedurally

**Editor Control**
- Start/stop Play mode
- Run Unity Test Runner tests
- Read console logs (errors, warnings) in real time

**Animations & VFX**
- Read/write animation controllers and clips
- Create particle systems

---

## What Still Requires Manual Work in Unity Editor

| Task | Why manual is better |
|---|---|
| Positioning objects visually in the scene | Dragging in Scene view is faster than typing coordinates |
| Adjusting collider shapes/sizes | Visual handles make this much easier |
| Setting up Camera framing | Visual preview needed |
| Importing new assets (sprites, audio, fonts) | Drag & drop into Project window |
| Animator state machine layout | Visual graph editor |
| Build & export to platform | Build Settings → Build |
| Testing actual gameplay feel | Play mode, controller in hand |

---

## Package Reference

**Unity MCP package** — installed via Package Manager (git URL):
```
https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main
```
