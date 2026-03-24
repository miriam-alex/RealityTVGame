using UnityEngine; 
using TMPro; 
using System.Collections;
using System.Collections.Generic;
public class PlayerIdentity : MonoBehaviour {
    [Header("Essentials")]
    public int playerIndex; 
    public PlayerRuntimeSet runtimeSet;
    public AnimalCatalog animalCatalog;
    public Transform bodyMountPoint;
    public string selectedAnimalId;
    [Header("Key Bindings")]
    public string interactKey = "E";
    public string altInteractKey = "F";
    public bool isSpotted;
    
    [Header("UI Feedback")]
    public Color spottedColor = Color.red;
    private Color normalColor = Color.white;
    private bool wasSpottedLastFrame;
    public TMP_Text scoreText; 
    void Awake()
    {
        Transform scoreTextTransform = transform.Find("Overhead Canvas/Score");
        if (scoreTextTransform != null)
        { 
            scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    void Start()
    {
        ApplyAnimalById(selectedAnimalId);
    }
    
    public void ApplyAnimalById(string id)
    {
        AnimalDefinition definition = animalCatalog.animals.Find(a => a.id == id);
        if (definition != null)
        {
            ApplyAnimal(definition);
        }
        else
        {
            Debug.LogError($"[PlayerIdentity] ID '{id}' not found in Catalog!");
        }
    }
    
    private void ApplyAnimal(AnimalDefinition definition)
    {
        if (bodyMountPoint == null)
        {
            Debug.LogError("[PlayerIdentity] bodyMountPoint is not assigned!");
            return;
        }

        // 1. Clear existing children (Clean the mount point)
        foreach (Transform child in bodyMountPoint)
        {
            Destroy(child.gameObject);
        }

        // 2. Instantiate the prefab as a child of the mount point
        GameObject newBody = Instantiate(definition.prefab, bodyMountPoint);
        
        // 3. Reset Transform to align with the Player container
        newBody.transform.localPosition = Vector3.zero;
        newBody.transform.localRotation = Quaternion.identity;

        // 4. Update the stable ID for persistence
        selectedAnimalId = definition.id;

        Debug.Log($"[PlayerIdentity] Player {playerIndex} is now a {selectedAnimalId}");
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

        scoreText.color = Color.white;
    }
}