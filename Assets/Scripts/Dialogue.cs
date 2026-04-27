using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem; // Added for Input System
using System;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float textSpeed;
    [SerializeField] private InputActionReference clickAction; // Drag your click action here

    private int index;
    private Coroutine typingCoroutine;
    private List<string> lines;
    public Action OnDialogueComplete;

    private void OnEnable()
    {
        if (clickAction != null)
        {
            clickAction.action.performed += OnGlobalClick;
            clickAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.action.performed -= OnGlobalClick;
        }
    }

    public void Initialize(List<string> givenLines)
    {
        // Pick a random line from the "nothing to say" list
        lines = givenLines;
        StartDialogue();
    }
    
    void StartDialogue()
    {
        index = 0;
        textComponent.text = string.Empty;
        typingCoroutine = StartCoroutine(TypeLine());
    }

    // This replaces OnPointerClick
    private void OnGlobalClick(InputAction.CallbackContext context)
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // If the text is still typing, finish it instantly
        if (textComponent.text != lines[index])
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            textComponent.text = lines[index];
        }
        // If the text is finished, go to the next line
        else
        {
            NextLine();
        }
    }
    
    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Count - 1)
        {
            index++;
            typingCoroutine = StartCoroutine(TypeLine());
        }
        else
        {
            OnDialogueComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}