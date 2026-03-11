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

                // Determine winner by highest score, break ties randomly
                var scoreManager = FindObjectOfType<ScoreManager>();
                var playerSet = scoreManager?.playerRuntimeSet;
                int winningPlayerId = 0;
                Color winningPlayerColor = Color.white;
                if (playerSet != null && playerSet.Items.Count > 0 && scoreManager != null)
                {
                    int maxScore = int.MinValue;
                    List<int> topPlayers = new List<int>();
                    for (int i = 0; i < playerSet.Items.Count; i++)
                    {
                        int score = scoreManager.GetScore(playerSet.Items[i]);
                        if (score > maxScore)
                        {
                            maxScore = score;
                            topPlayers.Clear();
                            topPlayers.Add(i);
                        }
                        else if (score == maxScore)
                        {
                            topPlayers.Add(i);
                        }
                    }
                    // Randomly select among top players if tie
                    winningPlayerId = topPlayers[Random.Range(0, topPlayers.Count)];
                    var winnerObj = playerSet.Items[winningPlayerId];
                    var winnerIdentity = winnerObj.GetComponent<PlayerIdentity>();
                    if (winnerIdentity != null)
                        winningPlayerColor = winnerIdentity.color;
                }

                GameResultData.WinnerId = winningPlayerId;
                GameResultData.WinnerColor = winningPlayerColor;

                SceneManager.LoadScene("PostGame");
            }
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
