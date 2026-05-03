using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private InputActionReference clickAction;
    [SerializeField] private string sceneToTransitionTo;
    
    private void OnEnable()
    {
        clickAction.action.performed += OnClickPerformed;
        clickAction.action.Enable();
    }
    
    private void OnDisable()
    {
        clickAction.action.performed -= OnClickPerformed;
        clickAction.action.Disable();
    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    
    public void OnClickPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Click performed");
        LoadScene(sceneToTransitionTo);
    }
}
