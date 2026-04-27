using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Cinemachine;

public class GameInitializer : MonoBehaviour
{
    [Tooltip("The name of the scene where the game logic should run.")]
    public string targetSceneName = "Lobby";
    
    [Header("Cameras")]
    [Tooltip("The virtual camera for the intro sequence.")]
    public GameObject introCamera;
    [Tooltip("How long the intro camera sequence should last.")]
    public float introDuration = 3.0f;

    public PlayerRuntimeSet runtimeSet;
    public Vector3[] spawnPoints;

    private GameObject[] _promptPanels;

    private bool _hasBeenInitialized = false;

    private CinemachineTargetGroup _targetGroup;

    private void Awake()
    {
        // Subscribe to the sceneLoaded event.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if the loaded scene is the one we want to initialize.
        if (scene.name == targetSceneName)
        {
            // Find the prompt panels in the scene.
            var promptContainer = FindAnyObjectByType<PromptPanelContainer>();
            if (promptContainer != null)
            {
                _promptPanels = promptContainer.promptPanels;
            }

            // Find the Cinemachine Target Group in the scene.
            _targetGroup = FindAnyObjectByType<CinemachineTargetGroup>();

            StartCoroutine(InitializeRoutine());
        }
    }

    private IEnumerator InitializeRoutine()
    {
        // Wait one frame for all other objects (like Players) to run their Awake/OnEnable/Start.
        yield return null; 

        // Make sure the intro camera is on at the start
        if (introCamera != null)
        {
            introCamera.SetActive(true);
        }
        
        InitializeGame();

        // Wait for the intro animation to finish
        yield return new WaitForSeconds(introDuration);

        // Then, disable the intro camera to switch to the main gameplay camera
        if (introCamera != null)
        {
            introCamera.SetActive(false);
        }

        // Set the main camera in the provider AFTER the switch
        CameraProvider.MainCamera = Camera.main;
    }

    public void InitializeGame()
    {
        if (_hasBeenInitialized) return;
        _hasBeenInitialized = true;

        // Clear existing player targets from the Cinemachine group.
        if (_targetGroup != null)
        {
            _targetGroup.m_Targets = new CinemachineTargetGroup.Target[0];
        }

        // 1. RENAME AND POSITION: Rename players for clarity, and move them to their spawn points.
        for (int i = 0; i < runtimeSet.Items.Count; i++)
        {
            if (i < spawnPoints.Length)
            {
                GameObject player = runtimeSet.Items[i];
                PlayerIdentity identity = player.GetComponent<PlayerIdentity>();

                // Rename the player object.
                if (identity != null)
                {
                    player.name = $"Player {identity.playerIndex + 1}";
                }
                
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                player.transform.position = spawnPoints[i];
                player.transform.rotation = Quaternion.identity;

                if (rb != null) rb.isKinematic = false;

                // Add player to the Cinemachine target group.
                if (_targetGroup != null)
                {
                    _targetGroup.AddMember(player.transform, 1f, 0.5f);
                }
            }
        }

        // 2. UI: Assign the prompt panels to the players.
        // foreach (var player in runtimeSet.Items)
        // {
        //     PlayerIdentity identity = player.GetComponent<PlayerIdentity>();
        //     if (identity == null) continue;
        //
        //     int playerIndex = identity.playerIndex;
        //     if (playerIndex < _promptPanels.Length)
        //     {
        //         PlayerInteract pInteraction = player.GetComponent<PlayerInteract>();
        //         if (pInteraction != null)
        //         {
        //             pInteraction.promptUI = _promptPanels[playerIndex].GetComponent<InteractionPromptUI>();
        //         }
        //     }
        // }

        // 3. REGISTRATION: Hook players into the Managers.
        foreach (GameObject p in runtimeSet.Items)
        {
            if (p == null) continue;
            DirectorManager.Instance?.RegisterPlayer(p.GetComponent<PlayerIdentity>());
        }

        // 4. SCORE SYSTEM: Tell the ScoreManager who is playing.
        ScoreManager.Instance?.InitializeScores();

        // // 5. CAMERAMAN: Tell the camera to find a target.
        // FindAnyObjectByType<CameramanNPC>()?.InitializeCameraman();

        // 6. STATIONS: Spawn the input stations.
        FindAnyObjectByType<PlayerStationManager>()?.SpawnStationsForPlayers();

        // 7. TIMER: Finally, start the game clock.
        var timer = FindAnyObjectByType<Timer>();
        if (timer != null)
        {
            timer.enabled = true;
            timer.StartTimer();
        }
        
        Debug.Log("<color=green>Game Initialized Successfully in scene: " + targetSceneName + "</color>");
    }
}