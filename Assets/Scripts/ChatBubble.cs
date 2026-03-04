using UnityEngine;
using TMPro;
using System.Collections;

public class ChatBubble : MonoBehaviour
{
    [Header("Timing Settings")]
    [SerializeField] private float defaultDisplayDuration = 3f;
    [SerializeField] private bool fadeOut = true;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;
    private float displayDuration;

    public static void Create(ChatBubble chatBubblePrefab, Vector3 localPosition, Transform parent, string text)
    {
        Create(chatBubblePrefab, localPosition, parent, text, chatBubblePrefab.defaultDisplayDuration);
    }

    public static void Create(ChatBubble chatBubblePrefab, Vector3 localPosition, Transform parent, string text, float duration)
    {
        ChatBubble newChatBubble = Instantiate(chatBubblePrefab, parent);
        Transform chatBubbleTransform = newChatBubble.transform;
        chatBubbleTransform.localPosition = localPosition;
        newChatBubble.displayDuration = duration;
        newChatBubble.Setup(text);
        newChatBubble.StartCoroutine(newChatBubble.HandleTiming());
    }

    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro =  transform.Find("Text").GetComponent<TextMeshPro>();
    }

    private IEnumerator HandleTiming()
    {
        // Wait for the display duration
        yield return new WaitForSeconds(displayDuration);
        
        if (fadeOut)
        {
            yield return StartCoroutine(FadeOut());
        }
        
        Destroy(gameObject);
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color originalBackgroundColor = backgroundSpriteRenderer.color;
        Color originalTextColor = textMeshPro.color;
        
        while (elapsedTime < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            
            backgroundSpriteRenderer.color = new Color(originalBackgroundColor.r, originalBackgroundColor.g, originalBackgroundColor.b, alpha);
            textMeshPro.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, alpha);
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    // void Start()
    // {
    //     Setup("Hello world. I'm going to take a big fat little nap.");
    // }

    void Setup(string text)
    {
        textMeshPro.SetText(text);
        textMeshPro.ForceMeshUpdate();
        Vector2 textSize = textMeshPro.GetRenderedValues(false);
        float horizontalPadding = 0.1f;
        float verticalPadding = 1f;
        Vector2 padding = new Vector2(horizontalPadding, verticalPadding);
        backgroundSpriteRenderer.size = textSize + padding;
    }
}
