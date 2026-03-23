using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton instance
    
    [Header("Settings")]
    public int giveReward = 10;
    public int stealPenalty = 20;
    public PlayerRuntimeSet playerRuntimeSet;
    private Dictionary<GameObject, int> playerScores = new Dictionary<GameObject, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        List<GameObject> activePlayers = playerRuntimeSet.Items;
        foreach (GameObject player in activePlayers)
        {
            playerScores[player] = 0;
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            if (id != null)
                id.UpdateScoreUI(0, true);
        }
    }
    
    public void PenalizeSteal(GameObject player)
    {
        AddScore(-stealPenalty, player);
    }
    
    public void RewardGive(GameObject player)
    {
        AddScore(giveReward, player);
    }

    public void RewardResources(GameObject player, ResourceItem resource)
    {
        AddScore(resource.PointsValue, player);
    }

    public void AddScore(int amount, GameObject player)
    {
        if (player == null) return;

        if (!playerScores.ContainsKey(player))
            playerScores[player] = 0;

        int oldScore = playerScores[player];
        playerScores[player] += amount;
        playerScores[player] = Mathf.Max(0, playerScores[player]);
        int newScore = playerScores[player];
        int delta = newScore - oldScore;

        PlayerIdentity id = player.GetComponent<PlayerIdentity>();
        if (id != null)
        {
            Debug.Log($"Player {id.playerIndex + 1} score: {newScore}");
            bool gainedPoints = delta > 0;
            id.UpdateScoreUI(newScore, gainedPoints);
        }
    }

    public int GetScore(GameObject player)
    {
        if (playerScores.ContainsKey(player))
            return playerScores[player];
        return 0;
    }
}
