using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep
    {
        Step1_Jump,
        Step2_CollectWater,
        Step3_ConvertToPlant,
        Step4_ConvertToFood,
        Step5_PracticeSteal,
        Step6_PracticeGive,
        Step7_StealOnCamera,
        Step8_ShowPlayback,
        Complete
    }

    [SerializeField] private Text tutorialUIText; // Assign a UI Text element in the Inspector
    [SerializeField] private PlayerRuntimeSet playerRuntimeSet; // Assign your PlayerRuntimeSet SO

    private TutorialStep currentStep;
    private Dictionary<GameObject, bool> stepCompletionStatus;

    void Start()
    {
        if (playerRuntimeSet == null || playerRuntimeSet.Items.Count == 0)
        {
            Debug.LogError("PlayerRuntimeSet is not assigned or is empty!");
            return;
        }

        InitializeStepCompletion();
        SetStep(TutorialStep.Step1_Jump);
    }

    void Update()
    {
        if (tutorialUIText == null) return;

        // This is where the logic for checking step completion will go.
        // For now, we'll just use a debug key to advance.
        if (Input.GetKeyDown(KeyCode.Space)) // TEMPORARY: Press Space to advance
        {
            AdvanceToNextStep();
        }
    }

    private void SetStep(TutorialStep newStep)
    {
        currentStep = newStep;
        ResetStepCompletion();

        switch (currentStep)
        {
            case TutorialStep.Step1_Jump:
                tutorialUIText.text = "Step 1: All players jump onto the mound!";
                // TODO: Add logic to detect when players are on the mound
                break;
            case TutorialStep.Step2_CollectWater:
                tutorialUIText.text = "Step 2: All players grab a water and drop it in your bin!";
                // TODO: Add logic to check bin contents
                break;
            case TutorialStep.Step3_ConvertToPlant:
                tutorialUIText.text = "Step 3: All players convert water to a plant and drop it in the bin!";
                // TODO: Add logic to check bin contents
                break;
            case TutorialStep.Step4_ConvertToFood:
                tutorialUIText.text = "Step 4: All players convert a plant to food and drop it in the bin!";
                // TODO: Add logic to check bin contents
                break;
            case TutorialStep.Step5_PracticeSteal:
                tutorialUIText.text = "Step 5: Practice stealing! Follow the instructions.";
                // TODO: Implement the circular stealing logic
                break;
            case TutorialStep.Step6_PracticeGive:
                tutorialUIText.text = "Step 6: Practice giving! Follow the instructions.";
                // TODO: Implement the circular giving logic
                break;
            case TutorialStep.Step7_StealOnCamera:
                tutorialUIText.text = "Step 7: A random player will be asked to steal on camera.";
                // TODO: Implement random player selection and camera logic
                break;

            case TutorialStep.Step8_ShowPlayback:
                tutorialUIText.text = "Step 8: Showing the playback.";
                // TODO: Trigger playback scene/logic
                break;

            case TutorialStep.Complete:
                tutorialUIText.text = "Tutorial Complete! Loading the main game...";
                LoadMainGame();
                break;
        }
    }

    public void MarkPlayerStepComplete(GameObject player)
    {
        if (stepCompletionStatus.ContainsKey(player))
        {
            stepCompletionStatus[player] = true;
            CheckForAllPlayersComplete();
        }
    }

    private void CheckForAllPlayersComplete()
    {
        if (stepCompletionStatus.Values.All(completed => completed))
        {
            AdvanceToNextStep();
        }
    }

    private void AdvanceToNextStep()
    {
        if (currentStep < TutorialStep.Complete)
        {
            SetStep(currentStep + 1);
        }
    }

    private void InitializeStepCompletion()
    {
        stepCompletionStatus = new Dictionary<GameObject, bool>();
        foreach (var player in playerRuntimeSet.Items)
        {
            stepCompletionStatus.Add(player, false);
        }
    }

    private void ResetStepCompletion()
    {
        var players = new List<GameObject>(stepCompletionStatus.Keys);
        foreach (var player in players)
        {
            stepCompletionStatus[player] = false;
        }
    }

    private void LoadMainGame()
    {
        // This will load your main game scene.
        SceneManager.LoadScene("GameScene");
    }
}
