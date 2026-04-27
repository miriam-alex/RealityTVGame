using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class CutsceneClickthrough : MonoBehaviour
{
    [SerializeField] private GameObject dialogueBoxPrefab;
    [SerializeField] private InputActionReference clickAction; // Drag your 'Left Click' action here
    [SerializeField] private Canvas canvasObj;
    [SerializeField] private List<string> cutsceneDialogue = new List<string>();
    
    private GameObject dialogueBox;
    [SerializeField] private string nextSceneName = "PlayerLobby";

    private void OnEnable()
    {
        // Subscribe to the "performed" event
        clickAction.action.performed += OnClickPerformed;
        clickAction.action.Enable();
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks or errors when the object is destroyed
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.Disable();
    }

    private void OnClickPerformed(InputAction.CallbackContext context)
    {
        if (dialogueBox == null)
        {
            CreateDialogueBox();
        }
    }

    void CreateDialogueBox()
    {
        if (canvasObj == null) return;

        dialogueBox = Instantiate(dialogueBoxPrefab, canvasObj.transform, false);
        Dialogue spawnedDialogue = dialogueBox.GetComponent<Dialogue>();

        if (spawnedDialogue != null)
        {
            spawnedDialogue.Initialize(cutsceneDialogue);
            spawnedDialogue.OnDialogueComplete = NextScene;
        }
    }
    
    private void NextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}