using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class Timer : MonoBehaviour
{
    public float timeRemaining = 120f;
    public bool timerRunning = false; 

    // Add this so GameInitializer can start the clock
    public void StartTimer()
    {
        timerRunning = true;
    }


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
        // Find the timer text in the scene. This is a bit brittle.
        // A better solution would be a reference passed in by the GameInitializer
        // or a more robust service locator pattern.
        var timerDisplay = FindAnyObjectByType<TimerDisplay>();
        if (timerDisplay != null)
        {
            timerText = timerDisplay.timerText;
        }
    }

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
        GameResultData.PlayerIndexToAnimalId.Clear();

        var scoreManager = ScoreManager.Instance;
        var playerSet = scoreManager?.playerRuntimeSet;
        if (playerSet == null || scoreManager == null) return;

        foreach (var playerObj in playerSet.Items)
        {
            if (playerObj == null) continue;
            var identity = playerObj.GetComponent<PlayerIdentity>();
            if (identity == null) continue;

            int playerIndex = identity.playerIndex;
            GameResultData.BaseScoresByPlayerIndex[playerIndex] = scoreManager.GetScore(playerObj);
            GameResultData.PlayerIndexToAnimalId[identity.playerIndex] = identity.selectedAnimalId;
            Debug.Log($"saved body prefab {GameResultData.PlayerIndexToAnimalId[playerIndex]} for player w index {playerIndex}");
            Debug.Log($"persisted player w index {playerIndex} for playback");
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
