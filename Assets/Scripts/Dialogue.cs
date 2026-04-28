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
            yield return new WaitForSeconds(textSpeed);
        }
    }

    void NextLine()
    {
        if (index < lines.Count - 1)
        {
            index++;
            typingCoroutine = StartCoroutine(TypeLine());
            PlayLineAudio(index);
        }
        else
        {
            if (audioSource != null) audioSource.Stop();
            OnDialogueComplete?.Invoke();
            Destroy(gameObject);
        }
    }
}