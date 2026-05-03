using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TutorialSkipManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject skipPopupUI;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference skipAction;    // Map to Left Shoulder (LB)
    [SerializeField] private InputActionReference confirmAction; // Map to Button South (A)
    [SerializeField] private InputActionReference cancelAction;  // Map to Button East (B)

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainGame"; // The name of your actual game scene

    private bool _isPopupActive = false;

    private void OnEnable()
    {
        // Enable and subscribe to the LB press
        if (skipAction != null)
        {
            skipAction.action.Enable();
            skipAction.action.performed += OnSkipPressed;
        }

        // Enable Confirm/Cancel actions
        if (confirmAction != null) confirmAction.action.Enable();
        if (cancelAction != null) cancelAction.action.Enable();
    }

    private void OnDisable()
    {
        if (skipAction != null) skipAction.action.performed -= OnSkipPressed;
    }

    private void Start()
    {
        // Ensure the pop-up is hidden at the start of the tutorial
        if (skipPopupUI != null)
            skipPopupUI.SetActive(false);
    }

    private void Update()
    {
        // Only listen for A/B if the pop-up is actually visible
        if (_isPopupActive)
        {
            if (confirmAction != null && confirmAction.action.triggered)
            {
                ConfirmSkip();
            }
            else if (cancelAction != null && cancelAction.action.triggered)
            {
                HidePopup();
            }
        }
    }

    private void OnSkipPressed(InputAction.CallbackContext context)
    {
        // Only show if it's not already open
        if (!_isPopupActive)
        {
            ShowPopup();
        }
    }

    public void ShowPopup()
    {
        _isPopupActive = true;
        if (skipPopupUI != null) skipPopupUI.SetActive(true);
        
        // Optional: Pause time if you want the tutorial to stop while deciding
        // Time.timeScale = 0f; 
    }

    public void HidePopup()
    {
        _isPopupActive = false;
        if (skipPopupUI != null) skipPopupUI.SetActive(false);
        
        // Time.timeScale = 1f;
    }

    public void ConfirmSkip()
    {
        // Reset time before switching scenes
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }
}