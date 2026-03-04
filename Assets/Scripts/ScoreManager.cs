using UnityEngine;
using TMPro; // Required for TextMeshPro UI elements
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance; // Singleton instance
    
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    public TMP_Text scoreTextP1; // Reference to the UI Text element
    public TMP_Text scoreTextP2; // Reference to the UI Text element
    public int stealAmount = 20;
    
    
    private Dictionary<int, TMP_Text> scoreTextDict;
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
        
        scoreTextDict = new Dictionary<int, TMP_Text>
        {
            { 0, scoreTextP1 },
            { 1, scoreTextP2 }
        };
        
        // Up to 4 players
        playerScores[0] = 0;
        playerScores[1] = 0;
        playerScores[2] = 0;
        playerScores[3] = 0;
        
        // Initialize score display at the start of the game
        UpdateScoreText();
    }

    public void AddScore(int amount, int playerIndex)
    {
        if (playerScores.ContainsKey(playerIndex))
        {
            playerScores[playerIndex] += amount;
            playerScores[playerIndex] = Mathf.Max(0, playerScores[playerIndex]);
            Debug.Log($"Player {playerIndex + 1} score: {playerScores[playerIndex]}");
        }
        UpdateScoreText();
    }

    public void StealPoints(int playerIndexFrom, int playerIndexTo)
    {
        if (playerScores.ContainsKey(playerIndexFrom) && playerScores.ContainsKey(playerIndexTo))
        {
            // We attempt to take up to the steal amount 
            int pointsStolen = Mathf.Min(stealAmount, playerScores[playerIndexFrom]);
            AddScore(-pointsStolen, playerIndexFrom);
            AddScore(pointsStolen, playerIndexTo);
        }
    }

    public void GivePoints(int playerIndexFrom, int playerIndexTo)
    {
        if (playerScores.ContainsKey(playerIndexFrom) && playerScores.ContainsKey(playerIndexTo))
        {
            // We attempt to give up to the steal amount 
            int pointsGiven = Mathf.Min(stealAmount, playerScores[playerIndexFrom]);
            AddScore(-pointsGiven, playerIndexFrom);
            AddScore(pointsGiven, playerIndexTo);
        }
        
    }


    // Updates the visual text element
    private void UpdateScoreText()
    {
        foreach (KeyValuePair<int, TMP_Text> entry in scoreTextDict)
        {
            if (entry.Value != null)
            {
                int playerIndex = entry.Key;
                int currentScore = playerScores[playerIndex];
                entry.Value.text = $"P{playerIndex + 1} Score: {playerScores[playerIndex].ToString()}";
            }
        }
    }
}