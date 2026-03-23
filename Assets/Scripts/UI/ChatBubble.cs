using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatBubble : MonoBehaviour
{
    // Track active bubbles by the object they are attached to
    private static Dictionary<Transform, ChatBubble> activeBubbles = new Dictionary<Transform, ChatBubble>();
    
    [Header("Settings")]
    public float defaultDisplayDuration = 0.25f; // Short duration for prompts

    [Header("Layout")]
    public float horizontalPadding = 0f;
    public float verticalPadding = 0.5f;

    private SpriteRenderer backgroundSpriteRenderer;
    private Image backgroundImage;
    private TMP_Text tmpText;

    public static void Clear(Transform parent)
    {
        if (parent == null) return;

        if (activeBubbles.TryGetValue(parent, out ChatBubble bubble) && bubble != null)
        {
            activeBubbles.Remove(parent);
            Destroy(bubble.gameObject);
        }
    }

    public static void Create(ChatBubble prefab, Vector3 localPosition, Transform parent, string text, float duration)
    {
        if (prefab == null || parent == null) return;

        // 1. If an active bubble exists for this specific parent, destroy it
        if (activeBubbles.ContainsKey(parent) && activeBubbles[parent] != null)
        {
            Destroy(activeBubbles[parent].gameObject);
        }

        ChatBubble newBubble = Instantiate(prefab, parent);
        newBubble.gameObject.SetActive(true);
        newBubble.transform.localPosition = localPosition;
        
        activeBubbles[parent] = newBubble;
        
        newBubble.Setup(text);

        float effectiveDuration = duration > 0f ? duration : newBubble.defaultDisplayDuration;
        newBubble.StartCoroutine(newBubble.HandleTiming(effectiveDuration, parent));
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
        ResolveReferences();
    }

    private void OnDestroy()
    {
        // Ensure static registry doesn't hold stale references.
        if (activeBubbles.Count == 0) return;

        List<Transform> keysToRemove = null;
        foreach (var kvp in activeBubbles)
        {
            if (kvp.Value == this)
            {
                keysToRemove ??= new List<Transform>();
                keysToRemove.Add(kvp.Key);
            }
        }

        if (keysToRemove != null)
        {
            foreach (var key in keysToRemove)
            {
                activeBubbles.Remove(key);
            }
        }
    }

    private void ResolveReferences()
    {
        if (tmpText == null)
        {
            // Prefer explicit child names when present, but fall back to a broad search.
            Transform textTransform = transform.Find("Text");
            tmpText = textTransform != null ? textTransform.GetComponent<TMP_Text>() : null;
            if (tmpText == null)
            {
                tmpText = GetComponentInChildren<TMP_Text>(true);
            }
        }

        if (backgroundSpriteRenderer == null && backgroundImage == null)
        {
            Transform bgTransform = transform.Find("Background");
            if (bgTransform != null)
            {
                backgroundSpriteRenderer = bgTransform.GetComponent<SpriteRenderer>();
                backgroundImage = bgTransform.GetComponent<Image>();
            }

            if (backgroundSpriteRenderer == null && backgroundImage == null)
            {
                backgroundSpriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
                backgroundImage = GetComponentInChildren<Image>(true);
            }
        }
    }

    private void Setup(string text)
    {
        ResolveReferences();

        if (tmpText == null)
        {
            Debug.LogWarning($"ChatBubble '{name}' is missing a TMP_Text child named 'Text' (or any TMP_Text in children).");
            return;
        }

        tmpText.SetText(text);
        tmpText.ForceMeshUpdate();

        // The 'false' argument looks at the pre-padding bounds.
        Vector2 textSize = tmpText.GetRenderedValues(false);

        // Background can be either SpriteRenderer (world) or Image (canvas).
        if (backgroundSpriteRenderer != null)
        {
            if (backgroundSpriteRenderer.drawMode == SpriteDrawMode.Simple)
            {
                backgroundSpriteRenderer.drawMode = SpriteDrawMode.Sliced;
            }
            backgroundSpriteRenderer.size = new Vector2(textSize.x + horizontalPadding, textSize.y + verticalPadding);
        }
        else if (backgroundImage != null)
        {
            RectTransform rt = backgroundImage.rectTransform;
            rt.sizeDelta = new Vector2(textSize.x + horizontalPadding, textSize.y + verticalPadding);
        }
        else
        {
            Debug.LogWarning($"ChatBubble '{name}' is missing a Background with SpriteRenderer or Image.");
        }
    }
}