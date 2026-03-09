using UnityEngine;

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
    
    [Header("Debug")]
    public int totalPointsCollected = 0;
    
    private void Start()
    {
        stationRenderer = GetComponent<Renderer>();
        if (stationRenderer == null)
        {
            stationRenderer = GetComponentInChildren<Renderer>();
        }
        
        // Set up input container if not assigned
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
        // Check for items in container and process them
        if (inputContainer != null && inputContainer.items.Count > 0)
        {
            ProcessItems();
        }
    }
    
    private void ProcessItems()
    {
        // Process all items in the container
        for (int i = inputContainer.items.Count - 1; i >= 0; i--)
        {
            ResourceItem item = inputContainer.items[i];
            if (item != null)
            {
                ConvertResourceToPoints(item);
                
                // Remove from container
                inputContainer.items.RemoveAt(i);
                
                // Destroy the physical item
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
        
        // Add points to player via ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(points, assignedPlayer);
            Debug.Log($"[{name}] Player {playerIndex + 1} gained {points} points from {resourceItem.ResourceName} (Total: {totalPointsCollected})");
        }
        else
        {
            Debug.LogError($"[{name}] ScoreManager.Instance is null!");
        }
    }
    
    public void AssignToPlayer(GameObject player)
    {
        assignedPlayer = player;
        
        PlayerIdentity playerIdentity = player?.GetComponent<PlayerIdentity>();
        if (playerIdentity != null)
        {
            playerIndex = playerIdentity.playerIndex;
            playerColor = GetPlayerColor(playerIndex);
            stationName = $"Player {playerIndex + 1} Collection Station";
            
            Debug.Log($"[{name}] Assigned to Player {playerIndex + 1}");
        }
        else
        {
            Debug.LogError($"[{name}] Assigned player has no PlayerIdentity component!");
        }
        
        UpdateVisualFeedback();
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
        
        // Update station name
        gameObject.name = stationName.Replace(" ", "");
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Show station info when player is near
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
