using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class PostGameScreen : MonoBehaviour
{
    public TMP_Text gameTitleText; 
    public AnimalCatalog animalCatalog;
    public List<Transform> playerPodiumPositions;

    public float zLoserOffset = 5f;

    void Start()
    {
        var sortedScores = GameResultData.BaseScoresByPlayerIndex
            .OrderByDescending(entry => entry.Value)
            .ToList();

        if (sortedScores.Count == 0) return;

        int highestScore = sortedScores[0].Value;
    
        var winners = sortedScores.Where(x => x.Value == highestScore).ToList();
        bool isTie = winners.Count > 1;

        if (isTie)
        {
            string winnerIndices = string.Join(", ", winners.Select(w => $"PLAYER {w.Key + 1}"));
            gameTitleText.text = $"{winnerIndices} TIE WITH {highestScore}K FOLLOWERS.";
        }
        else
        {
            gameTitleText.text = $"PLAYER {winners[0].Key + 1} WINS WITH {highestScore}K FOLLOWERS.";
        }

        for (int i = 0; i < sortedScores.Count; i++)
        {
            var entry = sortedScores[i];
            GameObject _playerObject = GetPlayerObject(entry.Key);
            GameObject visual = Instantiate(_playerObject);
        
            visual.transform.position = playerPodiumPositions[i].position;
            visual.transform.rotation = Quaternion.Euler(0, 180, 0);
        
            if (entry.Value != highestScore)
            {
                visual.transform.position += Vector3.forward * zLoserOffset;
            }
        }
    }
    
    private GameObject GetPlayerObject(int playerIndex)
    {
        GameObject _playerObject = null;
        if (GameResultData.PlayerIndexToAnimalId.TryGetValue(playerIndex, out string animalId))
        {
            if (animalCatalog != null)
            {
                AnimalDefinition definition = animalCatalog.animals.Find(a => a.id == animalId);
                if (definition != null && definition.prefab != null)
                {
                    Debug.Log($"[PostGameScreen] Resolved to animal ID: {animalId}");
                    _playerObject = definition.prefab;
                }
                else
                {
                    Debug.LogError($"[PostGameScreen] Catalog contains no prefab for ID: {animalId}");
                }
            }
            else
            {
                Debug.LogError("[PostGameScreen] AnimalCatalog is missing on PostGameScreen!");
            }
        }
        
        return _playerObject;
    }
}
