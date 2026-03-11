using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum DirectorPersonality { Indifferent, Judgmental, Aggressive }

public class CameramanNPC : MonoBehaviour
{
    [Header("Personality Settings")]
    public DirectorPersonality currentPersonality = DirectorPersonality.Indifferent;
    
    [Header("Movement")]
    public PlayerRuntimeSet runtimeSet;
    public Transform headTransform;
    public GameObject cone;
    private Renderer coneRenderer;
    private MeshFilter coneMeshFilter;
    public float baseMoveSpeed = 2.0f; // Slower "prowl" speed
    public float stoppingDistance = 6.0f;
    public float headRotationSpeed = 80f;

    [Header("Orbit Behavior")]
    public float orbitSpeed = 0.5f; // How fast he circles while watching
    
    private Transform currentTarget;
    private Coroutine resetCoroutine;
    private float personalityTimer;
    [SerializeField] private float detectionRadius = 1.0f;
    [SerializeField] private float detectionOffset = 1.0f;

    void Awake()
    {
        coneRenderer = cone.GetComponent<Renderer>();
        coneRenderer.material.color = Color.darkRed;
        coneMeshFilter = cone.GetComponent<MeshFilter>();
    }

    void Update()
    {
        ManagePersonality();
        MoveMechanically();
        RotateHead();
        DetectPlayers();
    }
    
    private void OnDrawGizmos()
    {
        if (cone.transform == null) return;
    
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(cone.transform.position + detectionOffset * Vector3.forward, detectionRadius);
    }
    
    private void DetectPlayers()
    {
        Vector3 center = cone.transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(center, detectionRadius);

        foreach (var player in runtimeSet.Items)
        {
            if (player.TryGetComponent(out PlayerIdentity id)) id.isSpotted = false;
        }

        // Now mark only the ones currently inside the beam
        bool playersSpotted = false;
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out PlayerIdentity id) || hit.GetComponentInParent<PlayerIdentity>())
            {
                var pId = hit.GetComponent<PlayerIdentity>() ?? hit.GetComponentInParent<PlayerIdentity>();
                pId.isSpotted = true;
                playersSpotted = true;
            }
        }

        coneRenderer.material.color = playersSpotted ? Color.green : Color.darkRed;
    }

    private void ManagePersonality()
    {
        if (currentPersonality == DirectorPersonality.Indifferent)
        {
            personalityTimer -= Time.deltaTime;
            if (personalityTimer <= 0 || currentTarget == null)
            {
                var players = runtimeSet.Items;
                if (players.Count > 0)
                {
                    currentTarget = players[Random.Range(0, players.Count)].transform;
                    personalityTimer = Random.Range(5f, 10f); 
                }
            }
        }
    }

    private void MoveMechanically()
    {
        if (currentTarget == null) return;

        float dist = Vector3.Distance(transform.position, currentTarget.position);
        
        // 1. DYNAMIC STATS BASED ON MODE
        float speed = baseMoveSpeed;
        float currentStopDist = stoppingDistance;

        if (currentPersonality == DirectorPersonality.Aggressive)
        {
            speed *= 2.0f;        // Move faster during drama
            currentStopDist *= 0.6f; // Get closer to the players
        }

        // 2. FORWARD MOVEMENT (Closing the gap)
        if (dist > currentStopDist + 0.5f)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, speed * Time.deltaTime);
        }
        // 3. ORBIT MOVEMENT (The "Always Moving" logic)
        else
        {
            // Rotate the NPC's position around the player so he never stops
            transform.RotateAround(currentTarget.position, Vector3.up, orbitSpeed * 20f * Time.deltaTime);
            
            // Gently maintain the distance so he doesn't drift away
            Vector3 desiredPos = (transform.position - currentTarget.position).normalized * currentStopDist + currentTarget.position;
            transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime);
        }
    }

    private void RotateHead()
    {
        if (currentTarget == null || headTransform == null) return;

        // 1. Aim for the "Chest" (1 unit up from pivot)
        Vector3 targetPoint = currentTarget.position + Vector3.up * 1.0f;
    
        // 2. Calculate direction from the HEAD's current position
        Vector3 direction = (targetPoint - headTransform.position).normalized;

        if (direction != Vector3.zero)
        {
            // 3. Create the target rotation
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // 4. Smoothly rotate. If this is still "wrong," 
            // your mesh is likely rotated 90 degrees inside the child object.
            headTransform.rotation = Quaternion.RotateTowards(
                headTransform.rotation, 
                targetRotation, 
                headRotationSpeed * (currentPersonality != DirectorPersonality.Indifferent ? 3f : 1f) * Time.deltaTime
            );
        }
    }

    public void SetDramaState(DirectorPersonality newPersonality, Transform target, float duration)
    {
        if (resetCoroutine != null) StopCoroutine(resetCoroutine);

        currentPersonality = newPersonality;
        currentTarget = target;
        resetCoroutine = StartCoroutine(ResetToIndifferent(duration));
    }

    private System.Collections.IEnumerator ResetToIndifferent(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentPersonality = DirectorPersonality.Indifferent;
    }
    
}