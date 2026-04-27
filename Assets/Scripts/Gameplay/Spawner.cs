using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject collectablePrefab; // Drag your prefab here in the Inspector
    public Vector3 spawnAreaMin = new Vector3(-10, 1, -10); // Minimum spawn coordinates
    public Vector3 spawnAreaMax = new Vector3(10, 1, 10); // Maximum spawn coordinates
    public float yValue = 0.45f;

    // Call this method to spawn a single object
    public void SpawnObject()
    {
        float randomX = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float randomZ = Random.Range(spawnAreaMin.z, spawnAreaMax.z);

        Vector3 randomSpawnPosition = new Vector3(randomX, yValue, randomZ);

        // Instantiate the prefab at the random position with no initial rotation
        Instantiate(collectablePrefab, randomSpawnPosition, Quaternion.identity);
    }

    // Example: Spawn an object every few seconds automatically
    void Start()
    {
        InvokeRepeating("SpawnObject", 2f, 5f); // Start spawning after 2s, repeat every 5s
        SpawnObject(); // Call once at the start
    }
}