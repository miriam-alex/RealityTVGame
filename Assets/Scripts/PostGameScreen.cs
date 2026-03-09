using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PostGameScreen : MonoBehaviour
{
    public TMP_Text gameTitleText; // Assign this to your 'gametitle' text in the Canvas
    public GameObject playerObject; // Assign this to your player GameObject in the scene

    void Start()
    {
        // Set the player object's color and get the correct player number
        int playerNumber = GameResultData.WinnerId + 1;
        if (playerObject != null)
        {
            var identity = playerObject.GetComponent<PlayerIdentity>();
            if (identity != null)
            {
                playerNumber = identity.playerIndex + 1;
            }
            var renderer = playerObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = GameResultData.WinnerColor;
            }
            else
            {
                var childRenderer = playerObject.GetComponentInChildren<MeshRenderer>();
                if (childRenderer != null)
                    childRenderer.material.color = GameResultData.WinnerColor;
            }
        }
        // Set the title text to "Congrats Player X" with the correct number
        gameTitleText.text = $"Congrats Player {playerNumber}";
    }
}
