using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class GameInitializer : MonoBehaviour
{
    [Header("Configuration")]
    public LevelSettings currentLevelSettings; // Will be auto-updated by LevelSettingsHolder if found
    
    [Header("Cameras")]
    public GameObject introCamera;
    public float introDuration = 3.0f;

    [Header("Data References")]
    public PlayerRuntimeSet runtimeSet;

    private GameObject[] _promptPanels;
    private bool _hasBeenInitialized = false;
    private CinemachineTargetGroup _targetGroup;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. Try to find scene-specific settings in the new scene
        LevelSettingsHolder holder = Object.FindFirstObjectByType<LevelSettingsHolder>();
        if (holder != null)
        {
            currentLevelSettings = holder.settings;
        }

        // 2. Only initialize if we have settings and the scene name matches
        if (currentLevelSettings != null && scene.name == currentLevelSettings.sceneName)
        {
            Debug.Log($"<color=cyan>GameInitializer:</color> Configuring for {scene.name}");
            
            // 3. Clean up any leftover routines from the previous scene/tutorial
            StopAllCoroutines();
            _hasBeenInitialized = false; 
            
            _targetGroup = Object.FindFirstObjectByType<CinemachineTargetGroup>();
            var promptContainer = Object.FindFirstObjectByType<PromptPanelContainer>();
            _promptPanels = (promptContainer != null) ? promptContainer.promptPanels : new GameObject[0];

            StartCoroutine(InitializeRoutine());
        }
    }

    private IEnumerator InitializeRoutine()
    {
        // Wait one frame for all objects to settle
        yield return null; 

        if (introCamera != null) introCamera.SetActive(true);
        
        InitializeGame();

        yield return new WaitForSeconds(introDuration);

        if (introCamera != null) introCamera.SetActive(false);
        CameraProvider.MainCamera = Camera.main;
    }

    public void InitializeGame()
    {
        if (_hasBeenInitialized) return;
        _hasBeenInitialized = true;

        if (_targetGroup != null)
        {
            _targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        }

        // Position players
        for (int i = 0; i < runtimeSet.Items.Count; i++)
        {
            if (i < currentLevelSettings.spawnPoints.Length)
            {
                GameObject player = runtimeSet.Items[i];
                
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                player.transform.position = currentLevelSettings.spawnPoints[i];
                player.transform.rotation = Quaternion.identity;

                if (rb) rb.isKinematic = false;

                if (_targetGroup != null)
                {
                    _targetGroup.AddMember(player.transform, 1f, 0.5f);
                }
            }
        }

        // Registration logic
        foreach (var player in runtimeSet.Items)
        {
            PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
            if (identity != null)
            {
                player.name = $"Player {identity.playerIndex + 1}";
                
                if (identity.playerIndex < _promptPanels.Length)
                {
                    PlayerInteract pInteraction = player.GetComponent<PlayerInteract>();
                    if (pInteraction) pInteraction.promptUI = _promptPanels[identity.playerIndex].GetComponent<InteractionPromptUI>();
                }
            }
            DirectorManager.Instance?.RegisterPlayer(player.GetComponent<PlayerIdentity>());
        }

        ScoreManager.Instance?.InitializeScores();
        Object.FindFirstObjectByType<PlayerStationManager>()?.SpawnStationsForPlayers();

        // 3. Timer Setup
        var timer = Object.FindFirstObjectByType<Timer>();
        if (timer != null)
        {
            timer.timeRemaining = currentLevelSettings.timeLimit;
            timer.enabled = true;
            timer.StartTimer();
        }
        
        Debug.Log($"<color=green>Successfully Initialized {currentLevelSettings.sceneName}</color>");
    }
}