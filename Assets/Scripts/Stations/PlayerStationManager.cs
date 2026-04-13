using UnityEngine;
using System.Collections.Generic;

public class PlayerStationManager : MonoBehaviour
{
    [Header("Station Spawning")]
    public GameObject inputStationPrefab;
    public PlayerRuntimeSet playerRuntimeSet;
    
    [Header("4 Player Spawn Positions")]
    public Vector3 player1Position = new Vector3(-5, 0.1f, -5);
    public Vector3 player2Position = new Vector3(5, 0.1f, -5);
    public Vector3 player3Position = new Vector3(-5, 0.1f, 5);
    public Vector3 player4Position = new Vector3(5, 0.1f, 5);
    
    private List<GameObject> spawnedStations = new List<GameObject>();

    public void SpawnStationsForPlayers()
    {
        Debug.Log("In SpawnStationsForPlayers");
        if (inputStationPrefab == null || playerRuntimeSet == null)
        {
            Debug.LogWarning("No player runtime set or input station prefab assigned");
            return;
        }

        // Clean up any old stations if this is called twice
        ClearExistingStations();

        Vector3[] positions = { player1Position, player2Position, player3Position, player4Position };
        
        // Use the actual count of players who joined in the lobby
        int playerCount = Mathf.Min(playerRuntimeSet.Items.Count, 4);
        
        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerObj = playerRuntimeSet.Items[i];
            if (playerObj == null)
            {
                Debug.LogWarning("Player object cannot be accessed from the runtime set");
            }

            PlayerIdentity playerIdentity = playerObj ? playerObj.GetComponent<PlayerIdentity>() : null;
            if (playerIdentity == null)
            {
                Debug.LogWarning("No player identity assigned");
            }
            
            // if there's no player identity, it's fine for playback.
            int playerIndex = playerIdentity ? playerIdentity.playerIndex : i;

            // Spawn station at the designated slot for this player index
            GameObject station = Instantiate(inputStationPrefab, positions[i], Quaternion.identity);
            station.name = $"InputStation_Player{playerIndex}";
            
            Color targetColor = GetPlayerColor(playerIndex);
            
            ApplyColorToStation(station, targetColor);
            
            InputStation inputStation = station.GetComponent<InputStation>();
            if (inputStation != null && playerObj != null) // assigning is not necessary for the playback
            {
                inputStation.AssignToPlayer(playerObj, targetColor); 
            }
            
            spawnedStations.Add(station);
            Debug.Log($"spawned station for player {playerIndex}");
        }
    }

    private void ClearExistingStations()
    {
        // Loop through the list of stations we tracked
        foreach (GameObject station in spawnedStations)
        {
            if (station != null) 
            {
                Destroy(station);
            }
        }
        // Empty the list so it's ready for a fresh spawn
        spawnedStations.Clear();
    }



    public Color GetPlayerColor(int playerIndex)
    {
        // Index 0: Pink, Index 1: Purple, Index 2: Green, Index 3: Yellow
        Color[] playerColors = { Color.magenta, new Color(0.5f, 0f, 1f), Color.green, Color.yellow };
        return playerIndex >= 0 && playerIndex < playerColors.Length ? playerColors[playerIndex] : Color.white;
    }

    void ApplyColorToStation(GameObject station, Color color)
    {
        Renderer[] renderers = station.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
                if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            }
        }
    }

    void OnDestroy()
    {
        ClearExistingStations();
    }
}