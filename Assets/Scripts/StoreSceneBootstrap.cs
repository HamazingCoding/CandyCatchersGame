using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

/// <summary>
/// Automatically bootstraps the StoreScene with the required UI components.
/// Can be attached to a GameObject in the scene, or works standalone via
/// RuntimeInitializeOnLoadMethod so it functions even if the scene file is empty.
/// </summary>
public class StoreSceneBootstrap : MonoBehaviour
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
        Debug.Log("[StoreSceneBootstrap] TryBootstrap called for scene: " + scene.name);

        if (scene.name != "StoreScene") return;

        Debug.Log("[StoreSceneBootstrap] StoreScene detected — bootstrapping...");

        // Ensure StoreUI exists
        if (Object.FindAnyObjectByType<StoreUI>() == null)
        {
            Debug.Log("[StoreSceneBootstrap] Creating StoreUI_Runtime GameObject.");
            var go = new GameObject("StoreUI_Runtime");
            go.AddComponent<StoreUI>();
        }
        else
        {
            Debug.Log("[StoreSceneBootstrap] StoreUI already exists in scene.");
        }

        // Ensure EventSystem exists
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            Debug.Log("[StoreSceneBootstrap] Creating EventSystem.");
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            esGO.AddComponent<StandaloneInputModule>();
#endif
        }

        Debug.Log("[StoreSceneBootstrap] Bootstrap complete.");
    }

    // ── Instance lifecycle (when attached to a GameObject) ──

    void Awake()
    {
        // If this MonoBehaviour is placed on a GameObject in the scene,
        // trigger the bootstrap immediately as a safety net.
        TryBootstrap(SceneManager.GetActiveScene());
    }
}

