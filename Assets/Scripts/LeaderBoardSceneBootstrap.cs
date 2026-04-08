using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Automatically bootstraps the LeaderboardScene with the required UI components.
/// Can be attached to a GameObject in the scene, or works standalone via
/// RuntimeInitializeOnLoadMethod so it functions even if the scene file is empty.
/// </summary>
public class LeaderboardSceneBootstrap : MonoBehaviour
{
    // ── Static auto-bootstrap (works without a GameObject in the scene) ──

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Also handle the case where we're already in the scene at startup
        TryBootstrap(SceneManager.GetActiveScene());
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBootstrap(scene);
    }

    static void TryBootstrap(Scene scene)
    {
        Debug.Log("[LeaderboardBootstrap] TryBootstrap called for scene: " + scene.name);

        if (scene.name != "LeaderboardScene") return;

        Debug.Log("[LeaderboardBootstrap] LeaderboardScene detected — bootstrapping...");

        // Create LeaderboardUI if not present
        if (Object.FindAnyObjectByType<LeaderboardUI>() == null)
        {
            Debug.Log("[LeaderboardBootstrap] Creating LeaderboardUI_Runtime GameObject.");
            var go = new GameObject("LeaderboardUI_Runtime");
            go.AddComponent<LeaderboardUI>();
        }
        else
        {
            Debug.Log("[LeaderboardBootstrap] LeaderboardUI already exists in scene.");
        }

        // Ensure an EventSystem exists for UI button clicks
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            Debug.Log("[LeaderboardBootstrap] Creating EventSystem.");
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        Debug.Log("[LeaderboardBootstrap] Bootstrap complete.");
    }

    // ── Instance lifecycle (when attached to a GameObject) ──

    void Awake()
    {
        // If this MonoBehaviour is placed on a GameObject in the scene,
        // trigger the bootstrap immediately as a safety net.
        TryBootstrap(SceneManager.GetActiveScene());
    }
}