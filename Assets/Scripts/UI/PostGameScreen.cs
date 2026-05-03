using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PostGameScreen : MonoBehaviour
{
    public TMP_Text gameTitleText; 
    public AnimalCatalog animalCatalog;
    public List<Transform> playerPodiumPositions;

    public float zLoserOffset = 5f;

    [SerializeField] private GameObject rollCreditsPromptRoot;
    [SerializeField] private TMP_Text pressACreditsText;
    [SerializeField] private GameObject creditsScrollPanel;
    [SerializeField] private InputActionReference pressACreditsAction;
    [SerializeField] private float rankingsDelay = 8f;
    [SerializeField] private string nextSceneName = "PreGame";
    [SerializeField] private bool singleContinueToNextScene = false;

    [SerializeField] private RectTransform creditsContent;
    [SerializeField] private float creditsScrollSpeed = 150f;

    [SerializeField] private GameObject darkOverlay;


    private enum CreditsFlowPhase
    {
        // enums for the different phases in PostGameScreen
        RankingsOnly,
        AwaitingRollCredits,
        CreditsOpen
    }

    // ranks -> press a -> credits
    private CreditsFlowPhase _creditsPhase = CreditsFlowPhase.RankingsOnly;
    private bool _continueInputHooked;


    private void OnDisable()
    {
        UnhookContinueInput();
    }
    void Start()
    {
        var sortedScores = GameResultData.BaseScoresByPlayerIndex
            .OrderByDescending(entry => entry.Value)
            .ToList();

        if (sortedScores.Count == 0) return;

        int highestScore = sortedScores[0].Value;
    
        var winners = sortedScores.Where(x => x.Value == highestScore).ToList();
        bool isTie = winners.Count > 1;

        if (gameTitleText != null)
        {
            if (isTie)
            {
                string winnerIndices = string.Join(", ", winners.Select(w => $"PLAYER {w.Key + 1}"));
                gameTitleText.text = $"{winnerIndices} TIE WITH {highestScore}K FOLLOWERS.";
            }
            else
            {
                gameTitleText.text = $"PLAYER {winners[0].Key + 1} WINS WITH {highestScore}K FOLLOWERS.";
            }
        }

        for (int i = 0; i < sortedScores.Count; i++)
        {
            var entry = sortedScores[i];
            GameObject _playerObject = GetPlayerObject(entry.Key);
            if (_playerObject == null) continue;
            GameObject visual = Instantiate(_playerObject);
        
            visual.transform.position = playerPodiumPositions[i].position;
            visual.transform.rotation = Quaternion.Euler(0, 180, 0);
        
            if (entry.Value != highestScore)
            {
                visual.transform.position += Vector3.forward * zLoserOffset;
            }
        }

        SetRollCreditsPromptVisible(false);
        if (creditsScrollPanel != null)
        {
            creditsScrollPanel.SetActive(false);
        }

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(false);
        }
        _creditsPhase = CreditsFlowPhase.RankingsOnly;

        if (HasRollCreditsPromptConfigured() || creditsScrollPanel != null)
        {
            StartCoroutine(ShowRollCreditsPromptAfterDelay());
        }
    }

    // checks if roll credits prompt is configured and if press a text is configured
    private bool HasRollCreditsPromptConfigured()
    {
        return rollCreditsPromptRoot != null || pressACreditsText != null;
    }

    // makes roll credits prompt or press a text visible
    private void SetRollCreditsPromptVisible(bool visible)
    {
        if (rollCreditsPromptRoot != null)
            rollCreditsPromptRoot.SetActive(visible);
        if (pressACreditsText != null)
            pressACreditsText.gameObject.SetActive(visible);
    }

    private IEnumerator ShowRollCreditsPromptAfterDelay()
    {
        yield return new WaitForSecondsRealtime(rankingsDelay);
        SetRollCreditsPromptVisible(true);

        if (darkOverlay != null)
        {
            darkOverlay.SetActive(true);
        }

        _creditsPhase = CreditsFlowPhase.AwaitingRollCredits;
        HookContinueInput();
    }

    private IEnumerator ScrollCreditsUpwards()
    {
        if (creditsContent == null)
        {
            Debug.LogError("[PostGameScreen] Credits content is not assigned");
            yield break;
        }

        RectTransform panelRect = creditsScrollPanel.GetComponent<RectTransform>();

        float startY = -(panelRect != null ? panelRect.rect.height : 600f);
        float endY = creditsContent.rect.height + 100f;

        creditsContent.anchoredPosition = new Vector2(0, startY);

        while (creditsContent.anchoredPosition.y < endY)
        {
            creditsContent.anchoredPosition += Vector2.up * creditsScrollSpeed * Time.deltaTime;
            yield return null;
        }

        UnhookContinueInput();
        SceneManager.LoadScene(nextSceneName);
    }

    // hooks continue input
    private void HookContinueInput()
    {
        if (_continueInputHooked || pressACreditsAction == null) return;
        pressACreditsAction.action.performed += OnContinuePressed;
        pressACreditsAction.action.Enable();
        _continueInputHooked = true;
    }

    private void UnhookContinueInput()
    {
        if (!_continueInputHooked || pressACreditsAction == null) return;
        pressACreditsAction.action.performed -= OnContinuePressed;
        pressACreditsAction.action.Disable();
        _continueInputHooked = false;
    }

    private void OnContinuePressed(InputAction.CallbackContext context)
    {
        if (_creditsPhase == CreditsFlowPhase.RankingsOnly) return;
        if (_creditsPhase == CreditsFlowPhase.AwaitingRollCredits)
        {
            // credits content is child of overall credits panel
            // upon pressing a, moves to credits content to be visible
            SetRollCreditsPromptVisible(false);

            if (singleContinueToNextScene)
            {
                UnhookContinueInput();
                SceneManager.LoadScene(nextSceneName);
                return;
            }
            if (creditsScrollPanel != null && rollCreditsPromptRoot != null && 
                creditsScrollPanel.transform.IsChildOf(rollCreditsPromptRoot.transform))
                {
                    Transform parent = rollCreditsPromptRoot.transform.parent;
                    if (parent != null)
                    {
                        creditsScrollPanel.transform.SetParent(parent, false);
                        creditsScrollPanel.transform.SetAsLastSibling();
                    }
                }

            if (creditsScrollPanel != null)
            {
                creditsScrollPanel.SetActive(true);
                var cg = creditsScrollPanel.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 1f;
                    cg.interactable = true;
                    cg.blocksRaycasts = true;
                }

                StartCoroutine(ScrollCreditsUpwards());
            }

            _creditsPhase = CreditsFlowPhase.CreditsOpen;
            return;
        }

        if (_creditsPhase == CreditsFlowPhase.CreditsOpen)
        {
            return;
            //UnhookContinueInput();
            //SceneManager.LoadScene(nextSceneName);
        }
    }
    
    private GameObject GetPlayerObject(int playerIndex)
    {
        GameObject _playerObject = null;
        if (GameResultData.PlayerIndexToAnimalId.TryGetValue(playerIndex, out string animalId))
        {
            if (animalCatalog != null)
            {
                AnimalDefinition definition = animalCatalog.animals.Find(a => a.id == animalId);
                if (definition != null && definition.prefab != null)
                {
                    Debug.Log($"[PostGameScreen] Resolved to animal ID: {animalId}");
                    _playerObject = definition.prefab;
                }
                else
                {
                    Debug.LogError($"[PostGameScreen] Catalog contains no prefab for ID: {animalId}");
                }
            }
            else
            {
                Debug.LogError("[PostGameScreen] AnimalCatalog is missing on PostGameScreen!");
            }
        }
        
        return _playerObject;
    }
}
