using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private AnimalCatalog catalog;

    // This is called by the PlayerInputManager 'Player Joined' Event
    public PlayerRuntimeSet runtimeSet;

    public void OnStartGameClicked()
    {
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

        if (playerInput == null) {
            Debug.LogError("LobbyManager: playerInput is NULL! Check your Event settings.");
            return;
        }

        PlayerIdentity identity = playerInput.GetComponent<PlayerIdentity>();
        
        if (identity == null) {
            Debug.LogError("LobbyManager: Could not find PlayerIdentity on the spawned prefab!");
            return;
        }

        int index = playerInput.playerIndex;
        Debug.Log($"LobbyManager: Processing Player Index {index}");

        if (catalog == null) {
            Debug.LogError("LobbyManager: AnimalCatalog is NOT ASSIGNED in the inspector!");
            return;
        }

        if (index < catalog.animals.Count)
        {
            string id = catalog.animals[index].id;
            identity.playerIndex = index;
            identity.selectedAnimalId = id; 
            
            Debug.Log($"LobbyManager: Assigning ID '{id}' to Player {index}");
            identity.ApplyAnimalById(id);
        }
        else {
            Debug.LogWarning($"LobbyManager: No animal found in catalog for index {index}");
        }
        
        DontDestroyOnLoad(playerInput.gameObject);
    }

}