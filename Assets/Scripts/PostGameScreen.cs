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
        if (playerObject == null)
        {
            return;
        }

        // Setting congratulations text
        var identity = playerObject.GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            playerNumber = identity.playerIndex + 1;
        }

        gameTitleText.text = $"Congrats Player {playerNumber}";

        // Setting player object's color
        Transform playerBodyTransform = playerObject.transform.Find("Player Body");
        if (playerBodyTransform == null)
        {
            Debug.LogWarning("Child named 'Player Body' not found!");
            return;
        }

        Transform bodyTransform = playerBodyTransform.Find("Body");
        if (bodyTransform != null)
        {
            MeshRenderer renderer = bodyTransform.GetComponent<MeshRenderer>();
            // Do something with the renderer
            if (renderer != null)
            {
                renderer.material.color = GameResultData.WinnerColor;
            }
        }
        else
        {
            Debug.LogWarning("Child named 'Body' not found!");
        }

        return;
    }
}
