using UnityEngine;

public class SpotlightDirector : MonoBehaviour
{
    public static SpotlightDirector Instance { get; private set; }
    [Header("Sweep Pattern")]
    public Vector3 arenaCenter;
    public Vector2 sweepRange = new Vector2(12f, 8f);
    public float sweepSpeed = 1.0f;
    
    [Header("Master Controls")]
    [Range(1f, 10f)] public float spotlightSize = 4f;
    public float ceilingHeight = 15f;

    [Header("References")]
    public Transform containerPivot;  
    public Transform groundIndicator; 
    public PlayerRuntimeSet runtimeSet;
    public LayerMask floorLayer; // IMPORTANT: Set this to your "Floor" layer

    [Header("Detection Settings")]
    public float baseRadius = 0.5f;

    [Header("Yodel Summon Settings")]
    public float yodelSummonDuration = 1.25f;
    public float yodelSummonSpeed = 10f;

    private float timer;
    private float yodelSummonUntilTime;
    private Vector3 yodelSummonPosition;
    private Vector3 yodelSummonTarget;
    private Vector3 currentFloorTarget;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            return;
        }

        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        timer += Time.deltaTime * sweepSpeed;
        Vector3 desiredFloorTarget;

        // sets spotlight to yodel summon position if yodel is active
        // instant move to yodel summon position (exactly where to go, not smoothed)
        if (Time.time < yodelSummonUntilTime)
        {
            desiredFloorTarget = yodelSummonPosition;
            
        }

        else
        {
            // 1. Calculate the Target Position
            float x = Mathf.Sin(timer) * sweepRange.x;
            float z = Mathf.Cos(timer * 0.5f) * sweepRange.y;
            desiredFloorTarget = arenaCenter + new Vector3(x, 0, z);
        }

        // smoothly move target to desired floor target
        currentFloorTarget = Vector3.MoveTowards(currentFloorTarget, desiredFloorTarget, yodelSummonSpeed * Time.deltaTime);


        // 2. POSITION GROUND INDICATOR (Anti-Twitch Logic)
        if (groundIndicator != null)
        {
            // We cast from slightly above the ceiling height to ensure we hit the floor
            // The 'floorLayer' ensures we don't hit the players or the cone mesh
            // changes to currentFloorTarget to move to players current position (smoothened)
            if (Physics.Raycast(new Vector3(currentFloorTarget.x, ceilingHeight + 1f, currentFloorTarget.z), Vector3.down, out RaycastHit hit, ceilingHeight + 10f, floorLayer))
            {
                groundIndicator.position = hit.point + Vector3.up * 0.05f;
            }
            else
            {
                // Fallback if we miss the floor
                groundIndicator.position = new Vector3(currentFloorTarget.x, 0, currentFloorTarget.z);
            }

            groundIndicator.localScale = new Vector3(spotlightSize, 0.01f, 1f);
        }

        // 3. TRANSFORM THE CONE BEAM
        if (containerPivot != null)
        {
            containerPivot.position = new Vector3(arenaCenter.x, ceilingHeight, arenaCenter.z);
            containerPivot.LookAt(groundIndicator.position);

            float distance = Vector3.Distance(containerPivot.position, groundIndicator.position);
            // Sync beam thickness and length
            containerPivot.localScale = new Vector3(spotlightSize, spotlightSize, distance);
        }

        DetectPlayers(groundIndicator.position);
    }

    // summons spotlight to a specific position for a duration when yodel is pressed
    public void SummonToPosition(Vector3 worldPosition)
    {
        yodelSummonPosition = worldPosition;
        yodelSummonUntilTime = Time.time + yodelSummonDuration;
        Debug.Log("Summoning spotlight to position: " + worldPosition);
    }

    // currentFloorTarget is set to arenaCenter at start
    private void Start()
    {
        currentFloorTarget = arenaCenter;
    }

    private void DetectPlayers(Vector3 center)
    {
        float actualDetectionRadius = baseRadius * spotlightSize;

        foreach (var p in runtimeSet.Items)
        {
            if (p.TryGetComponent(out PlayerIdentity id)) id.isSpotted = false;
        }

        Collider[] hits = Physics.OverlapSphere(center, actualDetectionRadius);
        foreach (var hit in hits)
        {
            var id = hit.GetComponent<PlayerIdentity>() ?? hit.GetComponentInParent<PlayerIdentity>();
            if (id != null) id.isSpotted = true;
        }
    }
}