using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class Timer : MonoBehaviour
{
    public float timeRemaining = 120f;
    public bool timerRunning = true;

    public TMP_Text timerText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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

                // Persist base scores/colors for playback -> postgame.
                PersistBaseScoresForPlayback();

                // Playback will apply drama score impacts during the clip, compute winner, then transition.
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

        // Randomly select among top players if tie.
        return topPlayers[Random.Range(0, topPlayers.Count)];
    }

    private void PersistBaseScoresForPlayback()
    {
        GameResultData.BaseScoresByPlayerIndex.Clear();
        GameResultData.PlayerColorsByIndex.Clear();

        var scoreManager = FindObjectOfType<ScoreManager>();
        var playerSet = scoreManager?.playerRuntimeSet;
        if (playerSet == null || scoreManager == null) return;

        foreach (var playerObj in playerSet.Items)
        {
            if (playerObj == null) continue;
            var identity = playerObj.GetComponent<PlayerIdentity>();
            if (identity == null) continue;

            int playerIndex = identity.playerIndex;
            GameResultData.BaseScoresByPlayerIndex[playerIndex] = scoreManager.GetScore(playerObj);
            GameResultData.PlayerColorsByIndex[playerIndex] = identity.color;
        }
    }

    void DisplayTime(float time)
    {
        time += 1;
        
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
