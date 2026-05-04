using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStep { 
        Intro, PickupItem, WaterOnly, WaterAndPlant, WaterPlantAndFood, 
        StealCircle, GiveCircle, SpotlightAction, YodelFinal, Complete 
    }
    public TutorialStep currentStep = TutorialStep.Intro;

    [Header("UI & Dialogue Config")]
    public GameObject dialogueBoxPrefab; 
    public Canvas canvasObj;             
    public PlayerRuntimeSet playerRuntimeSet;
    
    [Header("Dialogue Content")]
    public List<string> introLines;
    public List<string> pickupLines; 
    public List<string> waterOnlyLines;
    public List<string> plantingLines;
    public List<string> cookingLines;
    public List<string> stealLines;
    public List<string> giveLines;
    public List<string> spotlightLines;
    public List<string> yodelLines;

    [Header("Station References")]
    public GameObject plantingStation;
    public GameObject cookingStation;
    
    [Header("Spotlight Reference")]
    public GameObject spotlight;

    [Header("Arrows")]
    public GameObject arrowPickupStation;
    public GameObject arrowPlantingStation;
    public GameObject arrowCookingStation;
    public List<GameObject> playerBinArrows; 

    private HashSet<GameObject> playersWhoFinishedTask = new HashSet<GameObject>();

    private void Start()
    {
        // 1. Initialize world state
        if(plantingStation) plantingStation.SetActive(false);
        if(cookingStation) cookingStation.SetActive(false);
        if(spotlight) spotlight.SetActive(false);

        // 2. Start the intro sequence
        StartCoroutine(InitialPopupDelay());
        RefreshArrows();
    }

    private IEnumerator InitialPopupDelay()
    {
        // Wait one frame to ensure UI and Managers are initialized
        yield return null; 
        ShowPopup(introLines);
        
        // IMPORTANT: We do NOT call AdvanceStep() here anymore.
        // Instead, we manually move to PickupItem so the Update loop starts checking for picks.
        currentStep = TutorialStep.PickupItem;
        
        // If you want the Pickup instructions to show immediately after Intro, 
        // you can call ShowPopup(pickupLines) here or wait for the player to close the box.
        RefreshArrows();
        ShowPopup(pickupLines);
    }

    private void OnEnable() 
    {
        ScoreManager.OnScoreChanged += HandleScoreChanged;
        PlayerInteract.OnStealAction += HandleStealAction;
        PlayerInteract.OnGiveAction += HandleGiveAction;
        PlayerInteract.OnYodelAction += HandleYodelAction;
    }

    private void OnDisable() 
    {
        ScoreManager.OnScoreChanged -= HandleScoreChanged;
        PlayerInteract.OnStealAction -= HandleStealAction;
        PlayerInteract.OnGiveAction -= HandleGiveAction;
        PlayerInteract.OnYodelAction -= HandleYodelAction;
    }

    private void Update()
    {
        // Only check for pickups during the specific pickup step
        if (currentStep == TutorialStep.PickupItem) 
        {
            CheckPickupTask();
        }
    }

    // --- TASK LOGIC ---

    private void CheckPickupTask()
    {
        if (playerRuntimeSet == null || playerRuntimeSet.Items.Count == 0) return;

        // Check if all players in the runtime set are holding an item
        bool allPlayersHolding = playerRuntimeSet.Items.All(p => {
            var interactScript = p.GetComponent<PlayerInteract>(); 
            return interactScript != null && interactScript.isHoldingItem; 
        });

        if (allPlayersHolding) 
        {
            AdvanceStep();
        }
    }

    private void HandleScoreChanged(GameObject player, int newScore)
    {
        // Ignore scores if the tutorial is finished or in pickup phase
        if (currentStep == TutorialStep.Complete || currentStep == TutorialStep.PickupItem) return;
        
        // Logic for score-based steps (Water, Plant, Cook)
        if (newScore >= GetCurrentThreshold())
        {
            playersWhoFinishedTask.Add(player);

            // Turn off specific bin arrow for this player
            if (currentStep == TutorialStep.WaterOnly)
            {
                int playerIndex = playerRuntimeSet.Items.IndexOf(player);
                if (playerIndex >= 0 && playerIndex < playerBinArrows.Count)
                {
                    if (playerBinArrows[playerIndex] != null)
                        playerBinArrows[playerIndex].SetActive(false);
                }
            }
            CheckStepCompletion();
        }
    }


    private void HandleStealAction(GameObject thief, GameObject victim)
    {
        if (currentStep != TutorialStep.StealCircle && currentStep != TutorialStep.SpotlightAction) return;

        if (currentStep == TutorialStep.StealCircle)
        {
            playersWhoFinishedTask.Add(thief);
            CheckStepCompletion();
        }
        else if (currentStep == TutorialStep.SpotlightAction)
        {
            // Get the identity to check if they are currently "spotted" by the spotlight
            var identity = thief.GetComponent<PlayerIdentity>();
            
            // Progress if the thief is caught on camera during the act
            if (identity != null && identity.isSpotted)
            {
                playersWhoFinishedTask.Add(thief);
                Debug.Log($"[Tutorial] {thief.name} caught stealing on camera!");
                CheckStepCompletion();
            }

            
        }
    }

    private void HandleGiveAction(GameObject giver, GameObject receiver)
    {
        if (currentStep != TutorialStep.GiveCircle && currentStep != TutorialStep.SpotlightAction) return;

        if (currentStep == TutorialStep.GiveCircle)
        {
            playersWhoFinishedTask.Add(giver);
            CheckStepCompletion();
        }
        else if (currentStep == TutorialStep.SpotlightAction)
        {
            var identity = giver.GetComponent<PlayerIdentity>();

            // Progress if the giver is caught on camera during the act
            if (identity != null && identity.isSpotted)
            {
                playersWhoFinishedTask.Add(giver);
                Debug.Log($"[Tutorial] {giver.name} caught giving on camera!");
                CheckStepCompletion();
            }
        }
    }

    private void HandleYodelAction(GameObject player)
    {
        if (currentStep == TutorialStep.YodelFinal)
        {
            playersWhoFinishedTask.Add(player);
            CheckStepCompletion();
        }
    }

    private bool IsUnderSpotlight(GameObject obj)
    {
        return SpotlightDirector.Instance != null && SpotlightDirector.Instance.IsObjectLit(obj);
    }

    private void CheckStepCompletion()
    {
        int requiredCount = (currentStep == TutorialStep.SpotlightAction) ? 1 : playerRuntimeSet.Items.Count;

        if (playersWhoFinishedTask.Count >= requiredCount)
        {
            playersWhoFinishedTask.Clear();
            AdvanceStep();
        }
    }

    private void AdvanceStep()
    {
        if (currentStep == TutorialStep.Complete) return;


        currentStep++;


        if (currentStep == TutorialStep.Complete)
        {
            StartCoroutine(ShowFinalCongratsAndTransition());
            return;
        }
        List<string> nextLines = currentStep switch {
            TutorialStep.PickupItem => pickupLines,
            TutorialStep.WaterOnly => waterOnlyLines,
            TutorialStep.WaterAndPlant => plantingLines,
            TutorialStep.WaterPlantAndFood => cookingLines,
            TutorialStep.StealCircle => stealLines,
            TutorialStep.GiveCircle => giveLines,
            TutorialStep.SpotlightAction => spotlightLines,
            TutorialStep.YodelFinal => yodelLines,
            _ => null
        };

        // Station/Spotlight Persistence
        if (plantingStation) plantingStation.SetActive(currentStep >= TutorialStep.WaterAndPlant);
        if (cookingStation) cookingStation.SetActive(currentStep >= TutorialStep.WaterPlantAndFood);
        if (spotlight) spotlight.SetActive(currentStep >= TutorialStep.SpotlightAction);

        if (nextLines != null) ShowPopup(nextLines);
        RefreshArrows();
    }

    private IEnumerator ShowFinalCongratsAndTransition()
    {
        // Optional: wait a moment for the final yodel sound/effect to finish
        yield return new WaitForSeconds(1.0f);

        if (dialogueBoxPrefab == null || canvasObj == null) yield break;

        GameObject db = Instantiate(dialogueBoxPrefab, canvasObj.transform, false);
        Dialogue dialogue = db.GetComponent<Dialogue>();

        if (dialogue != null)
        {
            List<string> finalCongrats = new List<string> { 
                "Incredible! The audience loves you.", 
                "Now, let's see who can truly capture the spotlight.",
                "The show starts... NOW!" 
            };

            dialogue.Initialize(finalCongrats);

            // This triggers the scene load ONLY after the player clicks through the last line
            dialogue.OnDialogueComplete = () => {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene"); 
            };
        }
    }

    private void RefreshArrows()
    {
        if(arrowPickupStation) arrowPickupStation.SetActive(currentStep == TutorialStep.PickupItem);
        if(arrowPlantingStation) arrowPlantingStation.SetActive(currentStep == TutorialStep.WaterAndPlant);
        if(arrowCookingStation) arrowCookingStation.SetActive(currentStep == TutorialStep.WaterPlantAndFood);

        bool isWaterStep = (currentStep == TutorialStep.WaterOnly);
        int playerCount = playerRuntimeSet != null ? playerRuntimeSet.Items.Count : 0;
        for (int i = 0; i < playerBinArrows.Count; i++)
        {
            if (playerBinArrows[i]) 
                playerBinArrows[i].SetActive(isWaterStep && i < playerCount);
        }
    }

    private void ShowPopup(List<string> lines)
    {
        if (lines == null || lines.Count == 0 || canvasObj == null) return;
        
        GameObject db = Instantiate(dialogueBoxPrefab, canvasObj.transform, false);
        Dialogue dialogue = db.GetComponent<Dialogue>();
        if (dialogue != null) 
        {
            dialogue.Initialize(lines);
        }
    }

    private int GetCurrentThreshold()
    {
        return currentStep switch {
            TutorialStep.WaterOnly => 10,
            TutorialStep.WaterAndPlant => 30,
            TutorialStep.WaterPlantAndFood => 40,
            _ => 0 
        };
    }
}