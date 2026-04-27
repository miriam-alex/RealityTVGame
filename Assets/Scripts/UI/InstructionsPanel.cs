using UnityEngine;

public class InstructionsPanel : MonoBehaviour
{
    public GameObject instructionsPanel;

    public void OpenInstructionsPanel()
    {
        instructionsPanel.SetActive(true);
    }

    public void CloseInstructionsPanel()
    {
        instructionsPanel.SetActive(false);
    }
}
