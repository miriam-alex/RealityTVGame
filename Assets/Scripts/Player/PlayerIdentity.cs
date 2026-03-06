using UnityEngine; 
using TMPro; 
using System.Collections;
using System.Collections.Generic;
public class PlayerIdentity : MonoBehaviour {
    public int playerIndex; // Set this to 0 for P1, 1 for P2 in the Inspector
    public bool spotlightOn;
    public Color color;
    public PlayerRuntimeSet runtimeSet;
    private List<GameObject> activePlayers;
    private GameObject bodyObject;
    private TMP_Text scoreText; 
    void Start()
    {
        bodyObject = transform.Find("Player Body/Body").gameObject;
        if (bodyObject != null)
        {
            MeshRenderer meshRenderer = bodyObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null) 
                meshRenderer.material.color = color;
        }
        
        Transform scoreTextTransform = transform.Find("Overhead Canvas/Score");
        Debug.Log("found: " + scoreTextTransform.name);
        if (scoreTextTransform != null)
        { 
            scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
        }
        else
        {
            Debug.Log("Score Text Transform is null");
        }
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
    }

    void OnDisable() 
    {
        runtimeSet.Remove(this.gameObject);
    }
    
    public void UpdateScoreUI(int newScore, bool gainedPoints) 
    {
        // 1. Update the text
        scoreText.text = newScore.ToString();

        // 2. Restart the animation logic (Stop current one so they don't fight)
        StopAllCoroutines(); 
        StartCoroutine(AnimateScoreChange(gainedPoints));
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