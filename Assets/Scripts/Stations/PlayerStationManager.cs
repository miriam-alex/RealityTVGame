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

    void Start()
    {
        SpawnStationsForPlayers();
    }

    void SpawnStationsForPlayers()
    {
        if (inputStationPrefab == null || playerRuntimeSet == null) return;

        Vector3[] positions = { player1Position, player2Position, player3Position, player4Position };
        int playerCount = Mathf.Min(playerRuntimeSet.Items.Count, 4);
        
        for (int i = 0; i < playerCount; i++)
        {
            GameObject playerObj = playerRuntimeSet.Items[i];
            PlayerIdentity playerIdentity = playerObj.GetComponent<PlayerIdentity>();
            
            if (playerIdentity == null) continue;

            // Spawn station
            GameObject station = Instantiate(inputStationPrefab, positions[i], Quaternion.identity);
            station.name = $"InputStation_Player{playerIdentity.playerIndex + 1}";
            
            // Get the specific color for this player
            Color targetColor = GetPlayerColor(playerIdentity.playerIndex);
            
            // 1. Physically tint the station
            ApplyColorToStation(station, targetColor);
            
            // 2. Tell the script to use this color (Prevents it from resetting to Red/Blue)
            InputStation inputStation = station.GetComponent<InputStation>();
            if (inputStation != null)
            {
                inputStation.AssignToPlayer(playerObj, targetColor); 
            }
            
            spawnedStations.Add(station);
        }
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
        foreach (GameObject station in spawnedStations)
            if (station != null) Destroy(station);
    }
}