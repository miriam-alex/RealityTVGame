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
    private Dictionary<GameObject, int> playerCommunityScores = new Dictionary<GameObject, int>();
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: Keep the manager across scenes
            // DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        List<GameObject> activePlayers = playerRuntimeSet.Items;
        // Initializing mapping from Player -> Score
        foreach (GameObject player in activePlayers)
        {
            playerScores[player] = 0;
            playerCommunityScores[player] = 0;
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            id.UpdateScoreUI(0, 0, true);
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

    public void RewardResource(GameObject player, ResourceItem resource)
    {
        int amount = resource.PointsValue;
        AddScore(amount, player);
    }

    public void AddScore(int amount, GameObject player)
    {
        if (playerScores.ContainsKey(player))
        {
            playerScores[player] += amount;
            playerScores[player] = Mathf.Max(0, playerScores[player]);
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            Debug.Log($"Player {id.playerIndex + 1} score: {playerScores[player]}");

            bool gainedPoints = (amount > 0);
            id.UpdateScoreUI(playerScores[player],0,  gainedPoints);
        }
    }

    public int GetScore(GameObject player)
    {
        if (playerScores.ContainsKey(player))
            return playerScores[player];
        return 0;
    }

    public void AddCommunityScore(int amount, GameObject player)
    {
        if (playerCommunityScores.ContainsKey(player))
        {
            playerCommunityScores[player] += amount;
            playerCommunityScores[player] = Mathf.Max(0, playerCommunityScores[player]);
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            Debug.Log($"Player {id.playerIndex + 1} community score: {playerCommunityScores[player]}");

            bool gainedPoints = (amount > 0);
            id.UpdateScoreUI(0, playerCommunityScores[player], gainedPoints);
        }
    }
    
    public int GetCommunityScore(GameObject player)
    {
        if (playerCommunityScores.ContainsKey(player))
            return playerCommunityScores[player];
        return 0;
    }

    // public void TransferPoints(GameObject playerFrom, GameObject playerTo)
    // {
    //     if (playerScores.ContainsKey(playerFrom) && playerScores.ContainsKey(playerTo))
    //     {
    //         // We attempt to take up to the steal amount 
    //         int pointsStolen = Mathf.Min(stealAmount, playerScores[playerFrom]);
    //         AddScore(-pointsStolen, playerFrom);
    //         AddScore(pointsStolen, playerTo);
    //         PlayerIdentity playerFromId = playerFrom.GetComponent<PlayerIdentity>();
    //         PlayerIdentity playerToId = playerTo.GetComponent<PlayerIdentity>();
    //     }
    // }
    
}