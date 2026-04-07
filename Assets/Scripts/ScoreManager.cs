using UnityEngine;
using TMPro; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; 

    [Header("Settings")]
    public int giveReward = 10;
    public int stealPenalty = 20;
    
    private Dictionary<GameObject, int> playerScores = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Keep this commented out if you want a fresh manager per game
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // REMOVED: void Start() logic. 
    // We wait for GameInitializer to call InitializeScores().

    public void InitializeScores()
    {
        playerScores.Clear();
        
        PlayerIdentity[] players = FindObjectsOfType<PlayerIdentity>();
        if (players == null || players.Length == 0)
        {
            Debug.LogError("ScoreManager: No PlayerIdentity objects found!");
            return;
        }

        foreach (PlayerIdentity id in players)
        {
            if (id == null) continue;

            GameObject player = id.gameObject;
            playerScores[player] = 0;
            id.UpdateScoreUI(0, true);
            Debug.Log($"ScoreManager: Initialized Player {id.playerIndex}");
        }
        Debug.Log($"ScoreManager: Total players initialized: {playerScores.Count}");
    }
    
    public void AddScore(int amount, GameObject player)
    {
        if (playerScores.ContainsKey(player))
        {
            playerScores[player] += amount;
            playerScores[player] = Mathf.Max(0, playerScores[player]); // Prevent negative scores
            
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            if (id != null)
            {
                bool gainedPoints = (amount > 0);
                id.UpdateScoreUI(playerScores[player], gainedPoints);
            }
        }
        else 
        {
            // If this happens, it means InitializeScores() wasn't called properly!
            Debug.LogWarning($"ScoreManager: Attempted to add score to a player not in the dictionary!");
        }
    }

    public int GetScore(GameObject player)
    {
        if (playerScores.ContainsKey(player))
            return playerScores[player];
        return 0;
    }

    public IEnumerable<GameObject> GetPlayers()
    {
        return playerScores.Keys;
    }

    // Helpers for other scripts
    public void PenalizeSteal(GameObject player) => AddScore(-stealPenalty, player);
    public void RewardGive(GameObject player) => AddScore(giveReward, player);
    public void RewardResources(GameObject player, ResourceItem resource) => AddScore(resource.PointsValue, player);
}