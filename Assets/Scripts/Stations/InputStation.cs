using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class InputStation : MonoBehaviour
{
    [Header("Station Setup")]
    public Container inputContainer;
    public string stationName = "Resource Collection Station";
    
    [Header("Player Assignment")]
    public GameObject assignedPlayer;
    public int playerIndex = -1;
    
    [Header("Visual Feedback")]
    public Color playerColor = Color.white;
    private Renderer stationRenderer;
    private TextMeshPro stationNameText;
    
    [Header("Community Members")]
    public Transform[] memberSlots;
    public int pointsPerMember = 10;
    public int maxVisibleMembers = 8;
    
    [Header("Debug")]
    public int totalPointsCollected = 0;

    private List<GameObject> spawnedCommunityMembers = new List<GameObject>();

    private void Start()
    {
        stationRenderer = GetComponent<Renderer>();
        stationNameText = GetComponentInChildren<TextMeshPro>();
        
        if (stationRenderer == null)
        {
            stationRenderer = GetComponentInChildren<Renderer>();
        }
        
        if (inputContainer == null)
        {
            inputContainer = GetComponentInChildren<Container>();
        }
        
        if (inputContainer == null)
        {
            Debug.LogError($"[{name}] No Container found! InputStation needs a Container component.");
        }
        else
        {
            Debug.Log($"[{name}] InputStation initialized with container: {inputContainer.name}");
        }
        
        UpdateVisualFeedback();
    }

    
    private void Update()
    {
        if (inputContainer != null && inputContainer.items.Count > 0)
        {
            ProcessItems();
        }
    }

    // spawns community members for the player depending on score and animal prefab
    private void SpawnCommunityMembers()
    {
        foreach (var member in spawnedCommunityMembers)
        {
            if (member != null)
            {
                Destroy(member);
            }
        }

        spawnedCommunityMembers.Clear();

        if (memberSlots == null) return;

        // gathers player identity and animal definition
        PlayerIdentity identity = assignedPlayer?.GetComponent<PlayerIdentity>();
        if(identity == null || identity.animalCatalog == null) return;

        AnimalDefinition definition = identity.animalCatalog.animals.Find(a => a.id == identity.selectedAnimalId);

        // no animation definition found, return
        if (definition == null)
        {
            Debug.LogWarning($"[{name}] Could not find AnimalDefinition for player {identity.playerIndex}");
            return;
        }

        int slotsToUse = Mathf.Min(memberSlots.Length, maxVisibleMembers);

        // spawns community members in the slots
        for (int i = 0; i < slotsToUse; i++)
        {
            GameObject member = Instantiate(definition.prefab, memberSlots[i].position, memberSlots[i].rotation, memberSlots[i]);
            member.transform.localScale = Vector3.one * 0.1f;

            // disable animation on community members
            Animator[] animators = member.GetComponentsInChildren<Animator>(true);
            for (int j = 0; j < animators.Length; j++)
            {
                animators[j].enabled = false;
            }
            member.SetActive(false);
            spawnedCommunityMembers.Add(member);
        }

        Debug.Log($"[{name}] Spawned {slotsToUse} community members for player {identity.playerIndex}");
    }

    // calculates how many community members should be visible based on the player's score
    // right now while community members are showing up according to players score, there is a cap
    // on the community members array length
    // there is no unlimited amount of community members that can spawn
    // also, there is a discrepancy between when the score is decremented (members dont disappear)
    private void UpdateCommunityMembersFromScore()
    {
        // no score manager or player assigned, return
        if (ScoreManager.Instance == null || assignedPlayer == null) return;

        // if there are no communtiy members, return
        if (spawnedCommunityMembers.Count == 0) return;

        // get players score
        int score = ScoreManager.Instance.GetScore(assignedPlayer);
        // calculate how many community members should be visible
        int target = Mathf.FloorToInt(score / (float)pointsPerMember);
        target = Mathf.Clamp(target, 0, maxVisibleMembers);
        target = Mathf.Min(target, spawnedCommunityMembers.Count);

        // activates community members based on the target score
        // WILL FIX: this is just for testing purposes
        for (int i = 0; i < spawnedCommunityMembers.Count; i++)
        {
            if (spawnedCommunityMembers[i] != null)
            {
                bool isActive = i < target;
                if (isActive && !spawnedCommunityMembers[i].activeSelf)
                {
                    spawnedCommunityMembers[i].SetActive(true);
                    StartCoroutine(RandomJumpAnimation(spawnedCommunityMembers[i], memberSlots[i]));
                }
                //spawnedCommunityMembers[i].SetActive(i < target);

                else if (!isActive)
                {
                    spawnedCommunityMembers[i].SetActive(false);
                }

            }
        }
    }

    private IEnumerator RandomJumpAnimation(GameObject member, Transform slot)
    {
        float offset = Random.Range(0f, Mathf.PI * 2f);
        float speed = Random.Range(4f, 8f);
        float height = Random.Range(0.05f, 0.15f);

        while (member != null && member.activeSelf)
        {
            float y = Mathf.Sin((Time.time + offset) * speed) * height;
            member.transform.position = slot.position + new Vector3(0, y, 0);
            yield return null;
        }
    }
    
    private void ProcessItems()
    {
        for (int i = inputContainer.items.Count - 1; i >= 0; i--)
        {
            ResourceItem item = inputContainer.items[i];
            if (item != null)
            {
                ConvertResourceToPoints(item);
                
                inputContainer.items.RemoveAt(i);
                
                Destroy(item.gameObject);
            }
        }
    }
    
    private void ConvertResourceToPoints(ResourceItem resourceItem)
    {
        if (resourceItem?.resource == null)
        {
            Debug.LogWarning($"[{name}] ResourceItem has no resource assigned!");
            return;
        }
        
        if (assignedPlayer == null)
        {
            Debug.LogWarning($"[{name}] No player assigned to this station!");
            return;
        }
        
        int points = resourceItem.PointsValue;
        totalPointsCollected += points;
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.RewardResources(assignedPlayer, resourceItem);
            Debug.Log($"[{name}] Player {playerIndex + 1} gained {points} points from {resourceItem.ResourceName} (Total: {totalPointsCollected})");
        }
        else
        {
            Debug.LogError($"[{name}] ScoreManager.Instance is null!");
        }

        stationNameText.text = $"{totalPointsCollected}";
        UpdateCommunityMembersFromScore();
    }
    
    public void AssignToPlayer(GameObject player, Color color)
    {
        assignedPlayer = player;
        playerColor = color; // Use the color passed in from the Manager
        
        PlayerIdentity playerIdentity = player?.GetComponent<PlayerIdentity>();
        if (playerIdentity != null)
        {
            playerIndex = playerIdentity.playerIndex;
            stationName = $"Player {playerIndex + 1} Collection Station";

            SpawnCommunityMembers();
            
            Debug.Log($"[{name}] Assigned to Player {playerIndex + 1} with color {color}");
        }
        else
        {
            Debug.LogError($"[{name}] Assigned player has no PlayerIdentity component!");
        }
        
        UpdateVisualFeedback();
        UpdateCommunityMembersFromScore();
    }

    private Color GetPlayerColor(int index)
    {
        Color[] colors = {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.cyan,
            Color.magenta
        };
        
        return index >= 0 && index < colors.Length ? colors[index] : Color.white;
    }
    
    private void UpdateVisualFeedback()
    {
        if (stationRenderer != null)
        {
            stationRenderer.material.color = playerColor;
        }
        
        gameObject.name = stationName.Replace(" ", "");
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ShowStationInfo();
        }
    }
    
    private void ShowStationInfo()
    {
        Debug.Log($"=== {stationName} ===");
        Debug.Log($"• Assigned to: Player {playerIndex + 1}");
        Debug.Log($"• Total points collected: {totalPointsCollected}");
        Debug.Log($"• Items in container: {(inputContainer != null ? inputContainer.items.Count : 0)}");
    }
}
