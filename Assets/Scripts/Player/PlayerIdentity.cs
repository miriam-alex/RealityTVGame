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
    public TMP_Text scoreText; // Reference to the UI Text element
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
    
    public void UpdateScoreUI(int newScore) {
        scoreText.text = newScore.ToString();
        // You could also trigger a "BUMP" animation or color change here!
    }
}