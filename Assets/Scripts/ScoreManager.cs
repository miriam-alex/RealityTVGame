using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton instance
    
    public int stealAmount = 20;
    public PlayerRuntimeSet playerRuntimeSet;
    private Dictionary<GameObject, int> playerScores = new Dictionary<GameObject, int>();
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
            PlayerIdentity id = player.GetComponent<PlayerIdentity>();
            id.UpdateScoreUI(0, true);
        }
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
            id.UpdateScoreUI(playerScores[player], gainedPoints);
        }
    }

    public int GetScore(GameObject player)
    {
        if (playerScores.ContainsKey(player))
            return playerScores[player];
        return 0;
    }

    public void TransferPoints(GameObject playerFrom, GameObject playerTo)
    {
        if (playerScores.ContainsKey(playerFrom) && playerScores.ContainsKey(playerTo))
        {
            // We attempt to take up to the steal amount 
            int pointsStolen = Mathf.Min(stealAmount, playerScores[playerFrom]);
            AddScore(-pointsStolen, playerFrom);
            AddScore(pointsStolen, playerTo);
            PlayerIdentity playerFromId = playerFrom.GetComponent<PlayerIdentity>();
            PlayerIdentity playerToId = playerTo.GetComponent<PlayerIdentity>();
        }
    }
    
}