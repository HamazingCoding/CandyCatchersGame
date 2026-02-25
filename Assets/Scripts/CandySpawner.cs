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
    public Transform spawnParent;

    private Camera mainCam;
    private bool spawning = true;

    void Start()
    {
        mainCam = Camera.main;

        if (spawnParent == null)
            Debug.LogError("CandySpawner missing SpawnParent!");

        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (spawning)
        {
            SpawnOne();

            yield return new WaitForSeconds(spawnInterval);

            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval * spawnAcceleration);
        }
    }

    void SpawnOne()
    {
        float z = Mathf.Abs(mainCam.transform.position.z);

        Vector3 left = mainCam.ScreenToWorldPoint(new Vector3(0, 0, z));
        Vector3 right = mainCam.ScreenToWorldPoint(new Vector3(Screen.width, 0, z));

        float randomX = Random.Range(left.x + spawnPadding, right.x - spawnPadding);

        float topY = mainCam
            .ScreenToWorldPoint(new Vector3(0, Screen.height, z)).y + 1f;

        Vector3 pos = new Vector3(randomX, topY, 0f);

        bool spawnTrick = Random.value < 0.15f;
        GameObject prefab = spawnTrick ? GetRandomTrick() : GetRandomCandy();

        if (prefab == null) return;

        Instantiate(prefab, pos, Quaternion.identity, spawnParent);
    }

    GameObject GetRandomCandy()
    {
        return candyPrefabs[Random.Range(0, candyPrefabs.Length)];
    }

    GameObject GetRandomTrick()
    {
        return trickPrefabs[Random.Range(0, trickPrefabs.Length)];
    }

    public void StopSpawning()
    {
        spawning = false;
        StopAllCoroutines();
    }

    public void StartSpawning()
    {
        if (!spawning)
        {
            spawning = true;
            StartCoroutine(SpawnRoutine());
        }
    }
}
