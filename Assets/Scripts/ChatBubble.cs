using UnityEngine;
using TMPro;
public class ChatBubble : MonoBehaviour
{
    private SpriteRenderer backgroundSpriteRenderer;
    private TextMeshPro textMeshPro;

    public static void Create(ChatBubble chatBubblePrefab, Vector3 localPosition, Transform parent, string text)
    {
        ChatBubble newChatBubble = Instantiate(chatBubblePrefab, parent);
        Transform chatBubbleTransform = newChatBubble.transform;
        chatBubbleTransform.localPosition = localPosition;
        newChatBubble.Setup(text);
        Destroy(chatBubbleTransform.gameObject, 3f);
    }

    private void Awake()
    {
        backgroundSpriteRenderer = transform.Find("Background").GetComponent<SpriteRenderer>();
        textMeshPro =  transform.Find("Text").GetComponent<TextMeshPro>();
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
