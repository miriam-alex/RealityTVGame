using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameInitializer : MonoBehaviour
{
    [Tooltip("The name of the scene where the game logic should run.")]
    public string targetSceneName = "SampleScene";
    
    public PlayerRuntimeSet runtimeSet;
    public Vector3[] spawnPoints;

    private GameObject[] _promptPanels;

    private bool _hasBeenInitialized = false;

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

            StartCoroutine(InitializeRoutine());
        }
    }

    private IEnumerator InitializeRoutine()
    {
        // Wait one frame for all other objects (like Players) to run their Awake/OnEnable/Start.
        yield return null; 
        
        InitializeGame();
    }

    public void InitializeGame()
    {
        if (_hasBeenInitialized) return;
        _hasBeenInitialized = true;


        // 1. POSITIONING: Move the players now that we are in the correct scene.
        for (int i = 0; i < runtimeSet.Items.Count; i++)
        {
            if (i < spawnPoints.Length)
            {
                GameObject player = runtimeSet.Items[i];
                
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                player.transform.position = spawnPoints[i];
                player.transform.rotation = Quaternion.identity;

                if (rb != null) rb.isKinematic = false;
            }
        }

        // 2. REGISTRATION: Hook players into the Managers.
        foreach (GameObject p in runtimeSet.Items)
        {
            if (p == null) continue;
            DirectorManager.Instance?.RegisterPlayer(p.GetComponent<PlayerIdentity>());
        }

        // 3. UI: Assign the prompt panels to the players.
        for (int i = 0; i < runtimeSet.Items.Count; i++)
        {
            if (i < _promptPanels.Length)
            {
                GameObject player = runtimeSet.Items[i];
                PlayerInteract pInteraction = player.GetComponent<PlayerInteract>();
                if (pInteraction != null)
                {
                    pInteraction.promptUI = _promptPanels[i].GetComponent<InteractionPromptUI>();
                }
            }
        }

        // 4. SCORE SYSTEM: Tell the ScoreManager who is playing.
        ScoreManager.Instance?.InitializeScores();

        // 5. CAMERAMAN: Tell the camera to find a target.
        FindAnyObjectByType<CameramanNPC>()?.InitializeCameraman();

        // 6. STATIONS: Spawn the input stations.
        FindAnyObjectByType<PlayerStationManager>()?.SpawnStationsForPlayers();

        // 7. TIMER: Finally, start the game clock.
        FindAnyObjectByType<Timer>()?.StartTimer();
        
        // 8. INTERACTIONS: Reset the InteractionCoordinator.
        InteractionCoordinator.Instance?.Reset();

        Debug.Log("<color=green>Game Initialized Successfully in scene: " + targetSceneName + "</color>");
    }
}