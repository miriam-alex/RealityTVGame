using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ChatBubbleManager : MonoBehaviour
{
    // --- Singleton Logic ---
    private static ChatBubbleManager _instance;
    public static ChatBubbleManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to find it in the scene
                _instance = FindFirstObjectByType<ChatBubbleManager>();

                // If not found, load from Resources and create it
                if (_instance == null)
                {
                    GameObject prefab = Resources.Load<GameObject>("ChatBubbleManager");
                    if (prefab != null)
                    {
                        _instance = Instantiate(prefab).GetComponent<ChatBubbleManager>();
                    }
                    else
                    {
                        Debug.LogError("ChatBubbleManager prefab not found in Resources folder!");
                    }
                }
            }
            return _instance;
        }
    }

    [Header("Asset Reference")]
    [SerializeField] private GameObject bubblePrefab;

    [Header("Default Settings")]
    public float defaultDuration = 2.0f;
    public float horizontalPadding = 0.5f;
    public float verticalPadding = 0.5f;

    // Track active bubbles by the parent transform
    private Dictionary<Transform, GameObject> activeBubbles = new Dictionary<Transform, GameObject>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// The main entry point to create a bubble.
    /// Usage: ChatBubbleManager.Show("Hello!", playerTransform);
    /// </summary>
    public static void Show(string text, Transform parent, Vector3 localOffset = default, float duration = -1f)
    {
        Instance.CreateBubble(text, parent, localOffset, duration);
    }

    private void CreateBubble(string text, Transform parent, Vector3 localOffset, float duration)
    {
        if (parent == null) return;

        // 1. Clear existing bubble on this parent
        Clear(parent);

        // 2. Instantiate and Setup
        GameObject bubbleObj = Instantiate(bubblePrefab, parent);
        bubbleObj.transform.localPosition = localOffset;
        
        activeBubbles[parent] = bubbleObj;

        // 3. Configure Visuals
        SetupVisuals(bubbleObj, text);

        // 4. Handle Lifecycle
        float finalDuration = duration > 0 ? duration : defaultDuration;
        StartCoroutine(HandleLifecycle(bubbleObj, parent, finalDuration));
    }

    public void Clear(Transform parent)
    {
        if (activeBubbles.TryGetValue(parent, out GameObject existingBubble))
        {
            if (existingBubble != null) Destroy(existingBubble);
            activeBubbles.Remove(parent);
        }
    }

    private void SetupVisuals(GameObject bubbleObj, string text)
    {
        TMP_Text tmpText = bubbleObj.GetComponentInChildren<TMP_Text>();
        if (tmpText == null) return;

        tmpText.SetText(text);
        tmpText.ForceMeshUpdate();
        Vector2 textSize = tmpText.GetRenderedValues(false);

        // Update Background (Handles both World Sprite and UI Image)
        SpriteRenderer sr = bubbleObj.GetComponentInChildren<SpriteRenderer>();
        Image img = bubbleObj.GetComponentInChildren<Image>();

        Vector2 finalSize = new Vector2(textSize.x + horizontalPadding, textSize.y + verticalPadding);

        if (sr != null)
        {
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.size = finalSize;
        }
        else if (img != null)
        {
            img.rectTransform.sizeDelta = finalSize;
        }
    }

    private IEnumerator HandleLifecycle(GameObject bubble, Transform parent, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (bubble != null)
        {
            if (activeBubbles.ContainsKey(parent) && activeBubbles[parent] == bubble)
            {
                activeBubbles.Remove(parent);
            }
            Destroy(bubble);
        }
    }
}