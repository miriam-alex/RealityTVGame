using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject root;

    [Header("Primary")]
    [SerializeField] private TMP_Text primaryKeyText;
    [SerializeField] private TMP_Text primaryActionText;

    [Header("Secondary")]
    [SerializeField] private GameObject secondaryContainer;
    [SerializeField] private TMP_Text secondaryKeyText;
    [SerializeField] private TMP_Text secondaryActionText;

    private void Reset()
    {
        root = gameObject;
    }

    private void Awake()
    {
        if (root == null) root = gameObject;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(
        string primaryKey,
        string primaryAction,
        string secondaryKey,
        string secondaryAction,
        bool hasSecondary)
    {
        if (root != null && !root.activeSelf) root.SetActive(true);

        if (primaryKeyText != null) primaryKeyText.text = primaryKey;
        if (primaryActionText != null) primaryActionText.text = primaryAction;

        if (secondaryContainer != null) secondaryContainer.SetActive(hasSecondary);
        if (hasSecondary)
        {
            if (secondaryKeyText != null) secondaryKeyText.text = secondaryKey;
            if (secondaryActionText != null) secondaryActionText.text = secondaryAction;
        }
    }
}
