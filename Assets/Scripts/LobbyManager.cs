using UnityEngine;
using UnityEngine.UI; // Required for the Button component
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private AnimalCatalog catalog;
    [SerializeField] private Button startButton; // Drag your UI Button here in the Inspector

    public PlayerRuntimeSet runtimeSet;

    // We no longer strictly need an event trigger because we check the count every frame
    private void Update()
    {
        UpdateStartButtonState();
    }

    private void UpdateStartButtonState()
    {
        if (startButton != null)
        {
            // Enable button if 2 or more players are in the set
            // You can also add && runtimeSet.Items.Count <= 4 for a max limit
            bool canStart = runtimeSet.Items.Count >= 2;
            
            // Only update the interactable state if it changes to save performance
            if (startButton.interactable != canStart)
            {
                startButton.interactable = canStart;
            }
        }
    }

    public void OnStartGameClicked()
    {
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

    // This method is still useful for your Identity/Catalog assignment logic
    // but the button state is now handled automatically by Update()
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        Debug.Log("<color=cyan>LobbyManager:</color> OnPlayerJoined triggered!");

        // 1. Ensure the player is in the runtime set
        if (!runtimeSet.Items.Contains(playerInput.gameObject))
        {
            runtimeSet.Items.Add(playerInput.gameObject);
        }

        // 2. Keep your existing Identity assignment logic
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
    }
}