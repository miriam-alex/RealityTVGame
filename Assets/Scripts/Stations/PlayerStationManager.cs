using UnityEngine;
using System.Collections.Generic;

public class PlayerStationManager : MonoBehaviour
{
    [Header("Station Spawning")]
    public GameObject inputStationPrefab;
    public PlayerRuntimeSet playerRuntimeSet;
    
    [Header("4 Player Spawn Positions")]
    public Vector3 player1Position = new Vector3(-5, 1, -5);
    public Vector3 player2Position = new Vector3(5, 1, -5);
    public Vector3 player3Position = new Vector3(-5, 1, 5);
    public Vector3 player4Position = new Vector3(5, 1, 5);
    
    private List<GameObject> spawnedStations = new List<GameObject>();

    void Start()
    {
        SpawnStationsForPlayers();
    }

    void SpawnStationsForPlayers()
    {
        if (inputStationPrefab == null)
        {
            Debug.LogError("PlayerStationManager: No input station prefab assigned!");
            return;
        }

        if (playerRuntimeSet == null || playerRuntimeSet.Items.Count == 0)
        {
            Debug.LogWarning("PlayerStationManager: No active players found in PlayerRuntimeSet!");
            return;
        }

        // Get spawn positions array
        Vector3[] positions = { player1Position, player2Position, player3Position, player4Position };
        
        // Spawn one station per player (max 4)
        int playerCount = Mathf.Min(playerRuntimeSet.Items.Count, 4);
        
        for (int i = 0; i < playerCount; i++)
        {
            // Get the player GameObject
            GameObject playerObj = playerRuntimeSet.Items[i];
            PlayerIdentity playerIdentity = playerObj.GetComponent<PlayerIdentity>();
            
            if (playerIdentity == null)
            {
                Debug.LogError($"PlayerStationManager: Player {playerObj.name} has no PlayerIdentity component!");
                continue;
            }

            // Spawn station
            GameObject station = Instantiate(inputStationPrefab, positions[i], Quaternion.identity);
            station.name = $"InputStation_Player{playerIdentity.playerIndex + 1}";
            
            // Apply player color to station
            ApplyColorToStation(station, GetPlayerColor(playerIdentity.playerIndex));
            
            // Assign the station to this specific player
            InputStation inputStation = station.GetComponent<InputStation>();
            if (inputStation != null)
            {
                inputStation.AssignToPlayer(playerObj);
                Debug.Log($"PlayerStationManager: Spawned station for {playerObj.name} at {positions[i]}");
            }
            else
            {
                Debug.LogError($"PlayerStationManager: Station prefab is missing InputStation component!");
            }
            
            spawnedStations.Add(station);
        }

        Debug.Log($"PlayerStationManager: Successfully spawned {spawnedStations.Count} input stations");
    }

    Color GetPlayerColor(int playerIndex)
    {
        Color[] playerColors = { Color.magenta, new Color(0.5f, 0f, 1f), Color.green, Color.yellow };
        return playerIndex >= 0 && playerIndex < playerColors.Length ? playerColors[playerIndex] : Color.white;
    }

    void ApplyColorToStation(GameObject station, Color color)
    {
        // Find all renderers in the station and apply color
        Renderer[] renderers = station.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                // Try different URP material property names
                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                    Debug.Log($"Applied color {color} to _BaseColor property");
                }
                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", color);
                    Debug.Log($"Applied color {color} to _Color property");
                }
            }
        }
    }

    // Clean up stations if needed
    void OnDestroy()
    {
        foreach (GameObject station in spawnedStations)
        {
            if (station != null)
                DestroyImmediate(station);
        }
        spawnedStations.Clear();
    }

    // Gizmos to visualize spawn positions in editor
    void OnDrawGizmos()
    {
        Vector3[] positions = { player1Position, player2Position, player3Position, player4Position };
        
        for (int i = 0; i < positions.Length; i++)
        {
            Gizmos.color = GetPlayerColor(i);
            Gizmos.DrawWireCube(positions[i], Vector3.one);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(positions[i] + Vector3.up * 2f, $"Player {i + 1} Station");
            #endif
        }
    }
}