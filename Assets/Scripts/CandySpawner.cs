using UnityEngine;
using System.Collections;

public class CandySpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] candyPrefabs;
    public GameObject[] trickPrefabs;

    [Header("Spawn Settings")]
    public float spawnInterval = 1.0f;
    public float minSpawnInterval = 0.4f;
    public float spawnAcceleration = 0.98f;
    public float spawnPadding = 0.6f;
    public float trickChance = 0.15f;
    public Transform spawnParent;

    private Camera mainCam;
    private bool spawning = false;

    private int _totalCandiesToSpawn = 0;
    private int _candiesSpawned = 0;

    private bool _isEndless = false;
    private const float TrickChanceIncrement = 0.002f;
    private const float MaxTrickChance = 0.50f;

    // True once the spawn cap has been reached — GameManager polls this
    public bool IsDoneSpawning { get; private set; } = false;

    void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        if (spawnParent == null)
            Debug.LogError("CandySpawner missing SpawnParent!");

        // Spawning is started by ApplyConfig() (called from GameManager.Start).
        // Do NOT auto-start here — _totalCandiesToSpawn would still be 0.
    }

    IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            SpawnOne();
            _candiesSpawned++;

            // Stop once the level's total has been reached
            if (_totalCandiesToSpawn > 0 && _candiesSpawned >= _totalCandiesToSpawn)
            {
                spawning = false;
                IsDoneSpawning = true;
                // Notify GameManager so it can begin watching for screen-clear
                if (GameManager.Instance != null)
                    GameManager.Instance.OnSpawningComplete();
                yield break;
            }

            if (_isEndless)
                trickChance = Mathf.Min(trickChance + TrickChanceIncrement, MaxTrickChance);

            yield return new WaitForSeconds(spawnInterval);

            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * spawnAcceleration);
        }
    }

    void SpawnOne()
    {
        float z = Mathf.Abs(mainCam.transform.position.z);

        Vector3 left  = mainCam.ScreenToWorldPoint(new Vector3(0,            0, z));
        Vector3 right = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, z));

        float randomX = Random.Range(left.x + spawnPadding, right.x - spawnPadding);
        float topY    = mainCam.ScreenToWorldPoint(new Vector3(0, Screen.height, z)).y + 1f;

        Vector3 pos = new Vector3(randomX, topY, 0f);

        bool spawnTrick = Random.value < trickChance;
        GameObject prefab = spawnTrick ? GetRandomTrick() : GetRandomCandy();

        if (prefab == null) return;

        Instantiate(prefab, pos, Quaternion.identity, spawnParent);
    }

    GameObject GetRandomCandy() => candyPrefabs[Random.Range(0, candyPrefabs.Length)];
    GameObject GetRandomTrick() => trickPrefabs[Random.Range(0, trickPrefabs.Length)];

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
        // _candiesSpawned is intentionally NOT reset so a revive resumes from the same point
    }

    public void StartSpawning()
    {
        if (!IsDoneSpawning && !spawning)
        {
            spawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }

    /// <summary>Applied by GameManager from LevelConfig before spawning starts.</summary>
    public void ApplyConfig(LevelConfig cfg)
    {
        spawnInterval        = cfg.spawnInterval;
        minSpawnInterval     = cfg.minSpawnInterval;
        spawnAcceleration    = cfg.spawnAcceleration;
        trickChance          = cfg.trickChance;
        _totalCandiesToSpawn = cfg.totalCandiesToSpawn;
        _isEndless           = cfg.isEndless;
        _candiesSpawned      = 0;
        IsDoneSpawning       = false;

        // Start spawning now that config is applied with the correct cap
        spawning = true;
        StartCoroutine(SpawnRoutine());
    }
}
