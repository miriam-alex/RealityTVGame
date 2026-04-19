using UnityEngine;
using UnityEngine.UI; // Required for the Button component
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private AnimalCatalog catalog;
    [SerializeField] private Button startButton; // Drag your UI Button here in the Inspector

    public PlayerRuntimeSet runtimeSet;

    private void Start()
    {
        // Set initial state of the button when the lobby loads
        UpdateStartButtonState();
    }

    private void UpdateStartButtonState()
    {
        if (startButton != null)
        {
            int count = runtimeSet.Items.Count;
            Debug.Log($"LobbyManager: Current Player Count is {count}. Setting button interactable to {count >= 2}");
            
            startButton.interactable = count >= 2;
        }
    }

    public void OnStartGameClicked()
    {
        // Extra safety check to prevent accidental triggering
        if (runtimeSet.Items.Count < 2) return;

        // 1. Pack players for the trip
        foreach (GameObject player in runtimeSet.Items)
        {
            if (player != null) DontDestroyOnLoad(player);
        }
        runtimeSet.Items.RemoveAll(item => item == null);

        // 2. Switch scenes
        SceneManager.LoadScene("SampleScene"); 
    }

    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("<color=cyan>LobbyManager:</color> OnPlayerJoined triggered!");

        // 1. ADD THIS: Ensure the player is in the runtime set
        if (!runtimeSet.Items.Contains(playerInput.gameObject))
        {
            runtimeSet.Items.Add(playerInput.gameObject);
            Debug.Log("LobbyManager: Added player to runtimeSet. Current count: " + runtimeSet.Items.Count);
        }

        // 2. KEEP YOUR EXISTING IDENTITY LOGIC
        PlayerIdentity identity = playerInput.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            int index = playerInput.playerIndex;
            if (catalog != null && index < catalog.animals.Count)
            {
                string id = catalog.animals[index].id;
                identity.playerIndex = index;
                identity.selectedAnimalId = id; 
                identity.ApplyAnimalById(id);
            }
        }
        
        DontDestroyOnLoad(playerInput.gameObject);

        // 3. NOW update the button
        UpdateStartButtonState();
    }
}