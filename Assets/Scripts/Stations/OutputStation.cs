using UnityEngine;

public class OutputStation : MonoBehaviour
{
    [Header("Spawning Setup")]
    public Transform spawnPoint; // Use the Output Container's Transform here
    public ResourceItem resourcePrefab;
    public float generationInterval = 2f;
    
    [Header("Supply Settings")]
    public int maxSupply = -1; // -1 for unlimited
    public bool destroyWhenEmpty = false;

    private int currentSupply;
    private float timer;

    private void Start()
    {
        currentSupply = maxSupply;
        
        // Safety check for spawn point
        if (spawnPoint == null)
        {
            spawnPoint = transform;
            Debug.LogWarning($"[{name}] No spawn point assigned! Spawning at station center.");
        }
    }

    private void Update()
    {
        // Stop if we are out of supplies
        if (maxSupply != -1 && currentSupply <= 0)
        {
            if (destroyWhenEmpty) Destroy(gameObject);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= generationInterval)
        {
            timer = 0f;
            SpawnResource();
        }
    }

    private void SpawnResource()
    {
        if (resourcePrefab == null) return;

        // 1. Instantiate the object
        // We spawn slightly above the point (Vector3.up * 0.5f) so it drops onto the floor
        Vector3 finalSpawnPos = spawnPoint.position + (Vector3.up * 0.5f);
        ResourceItem obj = Instantiate(resourcePrefab, finalSpawnPos, Quaternion.identity);
        
        obj.transform.localScale = Vector3.one;

        // 2. PHYSICS HANDSHAKE:
        // We ensure the item is NOT kinematic so the player can pick it up immediately.
        // We DO NOT call Container.AddItem here to avoid the "weird binding" glitch.
        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Optional: Give it a tiny "pop" force so items don't stack perfectly
            rb.AddForce(Vector3.up * 1f, ForceMode.Impulse);
        }

        // 3. Decrement supply
        if (maxSupply != -1) currentSupply--;
    }
}