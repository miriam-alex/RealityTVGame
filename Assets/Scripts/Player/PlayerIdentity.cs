using UnityEngine; 
using TMPro; 
using System.Collections;
using System.Collections.Generic;
public class PlayerIdentity : MonoBehaviour {
    [Header("Essentials")]
    public int playerIndex; 
    public Color color;
    public PlayerRuntimeSet runtimeSet;
    [Header("Key Bindings")]
    public string interactKey = "E";
    public string altInteractKey = "F";
    public bool isSpotted;
    
    [Header("UI Feedback")]
    public Color spottedColor = Color.red;
    private Color normalColor = Color.white;
    private bool wasSpottedLastFrame;
    private GameObject bodyObject;
    public TMP_Text scoreText; 
    void Awake()
    {
        Transform bodyTransform = transform.Find("Player Body/Body");

        if (bodyTransform != null)
        {
            bodyObject = bodyTransform.gameObject;
        
            MeshRenderer meshRenderer = bodyObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null) 
                meshRenderer.material.color = color;
        }
        else
        {
            Debug.LogWarning($"[Setup Error] {gameObject.name} could not find 'Player Body/Body'. Is the hierarchy different in this scene?");
        }
        
        if (bodyObject != null)
        {
            MeshRenderer meshRenderer = bodyObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null) 
                meshRenderer.material.color = color;
        }
        
        Transform scoreTextTransform = transform.Find("Overhead Canvas/Score");
        if (scoreTextTransform != null)
        { 
            scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
        }
    }
    
    void Update()
    {
        HandleSpottedVisuals();
    }
    
    void OnEnable() 
    {
        if (runtimeSet == null) 
        {
            Debug.LogError($"<color=red>MISSING ASSET:</color> {gameObject.name} has no PlayerSet assigned!");
            return;
        }
    
        runtimeSet.Add(this.gameObject);
        Debug.Log($"<color=green>SUCCESS:</color> Added {gameObject.name}. Current count: {runtimeSet.Items.Count}");

        DirectorManager.Instance?.RegisterPlayer(this);
    }

    void OnDisable() 
    {
        runtimeSet.Remove(this.gameObject);

        DirectorManager.Instance?.UnregisterPlayer(this);
    }
    
    public void UpdateScoreUI(int newScore, bool gainedPoints) 
    {
        // 1. Update the text
        Debug.Log("newScore:"  + newScore);
        scoreText.text = newScore.ToString();

        // 2. Restart the animation logic (Stop current one so they don't fight)
        StopAllCoroutines(); 
        StartCoroutine(AnimateScoreChange(gainedPoints));
    }
    
    private void HandleSpottedVisuals()
    {
        if (!scoreText)
        {
            return;
        }

        // Only trigger changes when the state actually flips
        if (isSpotted != wasSpottedLastFrame)
        {
            if (isSpotted)
            {
                // Start a pulsating effect or change color immediately
                scoreText.text = "!!! " + scoreText.text + " !!!"; 
                scoreText.color = spottedColor;
                StartCoroutine(PulsateScore());
            }
            else
            {
                // Reset to normal
                scoreText.color = normalColor;
                scoreText.transform.localScale = Vector3.one;
                // Remove the exclamation marks
                scoreText.text = scoreText.text.Replace("!!! ", "").Replace(" !!!", "");
                StopCoroutine(PulsateScore());
            }
            wasSpottedLastFrame = isSpotted;
        }
    }

    private IEnumerator PulsateScore()
    {
        while (isSpotted)
        {
            float pulse = 1f + Mathf.PingPong(Time.time * 5f, 0.3f);
            scoreText.transform.localScale = new Vector3(pulse, pulse, pulse);
            yield return null;
        }
    }

    private IEnumerator AnimateScoreChange(bool gainedPoints) 
    {
        // Store original scale
        Vector3 originalScale = Vector3.one;
        Vector3 punchScale = Vector3.one * 1.5f; // Scale up by 50%

        // Flash color to Yellow
        if (gainedPoints)
        {
            scoreText.color = Color.green;
        }
        else
        {
            scoreText.color = Color.red;
        }

        // "Punch" animation: Scale up
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration) 
        {
            scoreText.transform.localScale = Vector3.Lerp(originalScale, punchScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Shrink animation: Scale back down
        elapsed = 0f;
        while (elapsed < duration) 
        {
            scoreText.transform.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Reset color to white
        scoreText.color = Color.white;
    }
}