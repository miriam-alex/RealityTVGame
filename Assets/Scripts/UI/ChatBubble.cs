using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ChatBubble : MonoBehaviour
{
    // Track active bubbles by the object they are attached to
    private static Dictionary<Transform, ChatBubble> activeBubbles = new Dictionary<Transform, ChatBubble>();

    [Header("Settings")]
    public float defaultDisplayDuration = 0.25f; // Short duration for prompts

    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;

    public static void Create(ChatBubble prefab, Vector3 localPosition, Transform parent, string text, float duration)
    {
        // 1. If an active bubble exists for this specific parent, destroy it
        if (activeBubbles.ContainsKey(parent) && activeBubbles[parent] != null)
        {
            Destroy(activeBubbles[parent].gameObject);
        }

        // 2. Instantiate new bubble
        ChatBubble newBubble = Instantiate(prefab, parent);
        newBubble.transform.localPosition = localPosition;
        
        // 3. Register this bubble
        activeBubbles[parent] = newBubble;
        
        newBubble.Setup(text);
        newBubble.StartCoroutine(newBubble.HandleTiming(duration, parent));
    }
    
    private System.Collections.IEnumerator HandleTiming(float duration, Transform parent)
    {
        yield return new WaitForSeconds(duration);
        
        // Remove from registry before destroying
        if (activeBubbles.ContainsKey(parent) && activeBubbles[parent] == this)
        {
            activeBubbles.Remove(parent);
        }
        Destroy(gameObject);
    }

    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro = transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private void Setup(string text)
    {
        textMeshPro.SetText(text);
    
        // 1. Force the mesh update to generate the geometry
        textMeshPro.ForceMeshUpdate();
    
        // 2. Get the rendered values of the text
        // The 'false' argument ensures we are looking at the pre-padding bounds
        Vector2 textSize = textMeshPro.GetRenderedValues(false);
    
        // 3. Define padding
        float horizontalPadding = 0f; 
        float verticalPadding = 0.5f;
    
        // 4. Update the background sprite size
        // We add the padding to the text bounds
        backgroundSpriteRenderer.size = new Vector2(textSize.x + horizontalPadding, textSize.y + verticalPadding);
    }
}