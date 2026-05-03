using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 120f;
    public bool timerRunning = false; 
    public TMP_Text timerText;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop the timer from running during the transition/intro
        timerRunning = false;

        var timerDisplay = Object.FindFirstObjectByType<TimerDisplay>();
        if (timerDisplay != null)
        {
            timerText = timerDisplay.timerText;
        }
    }

    public void StartTimer()
    {
        timerRunning = true;
        // Immediate UI refresh to prevent tutorial time from showing
        DisplayTime(timeRemaining);
    }

    void Update()
    {
        if (timerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;

                PersistBaseScoresForPlayback();
                DestroyPlayerObjects();
                SceneManager.LoadScene("Playback");
            }
        }
    }

    public static int DetermineWinnerId(Dictionary<int, int> scoresByPlayerIndex)
    {
        if (scoresByPlayerIndex == null || scoresByPlayerIndex.Count == 0)
            return 0;

        int maxScore = int.MinValue;
        List<int> topPlayers = new List<int>();

        foreach (var entry in scoresByPlayerIndex)
        {
            int playerIndex = entry.Key;
            int score = entry.Value;

            if (score > maxScore)
            {
                maxScore = score;
                topPlayers.Clear();
                topPlayers.Add(playerIndex);
            }
            else if (score == maxScore)
            {
                topPlayers.Add(playerIndex);
            }
        }

        return topPlayers[Random.Range(0, topPlayers.Count)];
    }

    private void PersistBaseScoresForPlayback()
    {
        GameResultData.BaseScoresByPlayerIndex.Clear();
        GameResultData.PlayerIndexToAnimalId.Clear();

        var scoreManager = ScoreManager.Instance;
        if (scoreManager == null) return;

        foreach (var playerObj in scoreManager.GetPlayers())
        {
            if (playerObj == null) continue;
            var identity = playerObj.GetComponent<PlayerIdentity>();
            if (identity == null) continue;

            int playerIndex = identity.playerIndex;
            GameResultData.BaseScoresByPlayerIndex[playerIndex] = scoreManager.GetScore(playerObj);
            GameResultData.PlayerIndexToAnimalId[identity.playerIndex] = identity.selectedAnimalId;
        }
    }

    private void DestroyPlayerObjects()
    {
        var scoreManager = ScoreManager.Instance;
        if (scoreManager == null) return;

        var playersToDestroy = new List<GameObject>(scoreManager.GetPlayers());
        foreach (var playerObj in playersToDestroy)
        {
            if (playerObj != null) Destroy(playerObj);
        }
    }

    void DisplayTime(float time)
    {
        // Floor it so 00:00 is exactly when the timer hits 0
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        
        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}