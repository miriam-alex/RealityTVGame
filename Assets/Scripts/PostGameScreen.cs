using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PostGameScreen : MonoBehaviour
{
    public TMP_Text gameTitleText; 
    public AnimalCatalog animalCatalog;
    private GameObject _playerObject;

    void Start()
    {
        if (GameResultData.PlayerIndexToAnimalId.TryGetValue(GameResultData.WinnerId, out string animalId))
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
        
        GameObject visual = Instantiate(_playerObject);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.Euler(0, 180,0);
        
        gameTitleText.text = $"PLAYER {GameResultData.WinnerId + 1} WINS!";

        return;
    }
}
