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

    // for narration audio
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private AudioSource audioSource;

    private int index;
    private Coroutine typingCoroutine;
    private List<string> lines;
    public Action OnDialogueComplete;

    // plays narration audio when dialogue is started
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

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
        PlayLineAudio(index);
    }

    // This replaces OnPointerClick
    private void OnGlobalClick(InputAction.CallbackContext context)
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // 1. SAFETY: Prevent the "NullReferenceException" you saw in the logs
        // by checking if lines are loaded before trying to read them.
        if (lines == null || lines.Count == 0 || index >= lines.Count) return;

        if (textComponent.text != lines[index])
        {
            // Finish typing current line instantly
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            textComponent.text = lines[index];
        }
        else
        {
            NextLine();
        }
    }


    // plays audio for each line of dialogue
    private void PlayLineAudio(int index)
    {
        if (audioSource == null ) return;
        if (audioClip == null) return;

        audioSource.Stop();
        audioSource.PlayOneShot(audioClip);
    }
    
    IEnumerator TypeLine()
    {
        textComponent.text = string.Empty;
        foreach (char c in lines[index].ToCharArray())
        {
            textComponent.text += c;
            // Use Realtime to keep UI timing consistent
            yield return new WaitForSecondsRealtime(textSpeed);
        }
    }
    void NextLine()
    {
        if (index < lines.Count - 1)
        {
            index++;
            textComponent.text = string.Empty;
            typingCoroutine = StartCoroutine(TypeLine());
            PlayLineAudio(index); //
        }
        else
        {
            // 2. THE CLEANUP: The player pressed A on the last line.
            if (audioSource != null) audioSource.Stop();
            
            // Notify any listeners (like TutorialManager) the box is gone
            OnDialogueComplete?.Invoke(); 
            
            // 3. THE DESTRUCTION: This removes the UI from the game state
            Destroy(gameObject); 
        }
    }
}