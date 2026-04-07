using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DirectorManager : MonoBehaviour 
{
    public static DirectorManager Instance;

    [Header("Episode Data")]
    // Movement data indexed by GameObject name (e.g., "Player_1", "MainCamera")
    public Dictionary<string, ActorTrack> productionLedger = new Dictionary<string, ActorTrack>();
    
    // List of every high-drama event (Thefts, Gifts, etc.)
    public List<DramaEvent> dramaRegistry = new List<DramaEvent>();

    [Header("Drama Logging")]
    [Tooltip("Prevents identical events (same type + actor + victim) from being logged repeatedly within this window.")]
    public float dramaDedupWindowSeconds = 0.75f;

    private readonly Dictionary<string, float> _lastDramaTimeByKey = new Dictionary<string, float>();

    [Header("Live Recording Registry")]
    private List<RealityActor> _activeActors = new List<RealityActor>();
    private readonly List<PlayerIdentity> _activePlayers = new List<PlayerIdentity>();

    [Header("Recording Control")]
    public bool isRecording = true;

    private float _episodeStartTime;
    private bool _episodeStartTimeInitialized;

    private void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Safety net: register actors after scene loads (covers script execution order issues).
            SceneManager.sceneLoaded += OnSceneLoaded;
        } 
        else 
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshActorRegistry();
    }

    private void RefreshActorRegistry()
    {
        RealityActor[] actors = Object.FindObjectsByType<RealityActor>(FindObjectsSortMode.None);
    
        foreach (var actor in actors)
        {
            // Note: FindObjectsByType only returns active/enabled objects by default,
            // so the null check is usually redundant but safe to keep.
            if (actor != null)
                RegisterActor(actor);
        }
    }
    public void SetRecording(bool shouldRecord)
    {
        isRecording = shouldRecord;
    }

    // --- REGISTRY MANAGEMENT ---
    
    public void RegisterActor(RealityActor actor) 
    {
        if (!_activeActors.Contains(actor)) 
            _activeActors.Add(actor);
    }

    public void UnregisterActor(RealityActor actor) 
    {
        if (_activeActors.Contains(actor)) 
            _activeActors.Remove(actor);
    }

    public void RegisterPlayer(PlayerIdentity player)
    {
        if (player == null) return;
        if (!_activePlayers.Contains(player))
            _activePlayers.Add(player);
    }

    public void UnregisterPlayer(PlayerIdentity player)
    {
        if (player == null) return;
        _activePlayers.Remove(player);
    }

    public void SetPlayerInput(bool enabled)
    {
        foreach (var player in _activePlayers)
        {
            if (player != null)
            {
                var controller = player.GetComponent<PlayerController>();
                if (controller != null)
                {
                    controller.enabled = enabled;
                }
            }
        }
    }

    // --- THE RECORDING LOOP ---

    private void FixedUpdate() 
    {
        if (!isRecording) return;

        if (!_episodeStartTimeInitialized)
        {
            _episodeStartTime = Time.time;
            _episodeStartTimeInitialized = true;
        }
        
        HashSet<string> recordedIdsThisFrame = new HashSet<string>();

        foreach (var actor in _activeActors) 
        {
            if (actor == null) continue;

            string id = actor.name;
            RecordedActorType actorType = RecordedActorType.NonPlayer;

            Transform recordTransform = actor.transform;
            PlayerIdentity playerIdentity = actor.GetComponentInParent<PlayerIdentity>();

            if (playerIdentity != null)
            {
                id = $"Player_{playerIdentity.playerIndex}";
                actorType = RecordedActorType.Player;

                // Record the player root transform, not the child RealityActor's transform.
                recordTransform = playerIdentity.transform;
            }

            if (recordedIdsThisFrame.Contains(id))
                continue;

            recordedIdsThisFrame.Add(id);

            if (!productionLedger.TryGetValue(id, out ActorTrack track))
            {
                track = new ActorTrack(id, actorType);
                productionLedger.Add(id, track);
            }
            else
            {
                // Backfill metadata if it wasn't known when the track was created.
                if (track.actorType == RecordedActorType.Unknown)
                    track.actorType = actorType;
            }

            // Log the frame with episode-relative time so playback can start at t=0.
            float relativeTime = Time.time - _episodeStartTime;
            track.frames.Add(new RealityFrame(recordTransform, relativeTime));
        }

        // Ensure players are always recorded, even if the prefab has no RealityActor.
        foreach (var player in _activePlayers)
        {
            if (player == null) continue;

            string id = $"Player_{player.playerIndex}";
            if (recordedIdsThisFrame.Contains(id))
                continue;

            recordedIdsThisFrame.Add(id);

            RecordedActorType actorType = RecordedActorType.Player;

            Transform body = player.transform.Find("Player Body/Body");
            if (!productionLedger.TryGetValue(id, out ActorTrack track))
            {
                track = new ActorTrack(id, actorType);
                productionLedger.Add(id, track);
            }
            else
            {
                track.actorType = RecordedActorType.Player;
            }

            float relativeTime = Time.time - _episodeStartTime;
            track.frames.Add(new RealityFrame(player.transform, relativeTime));
        }
    }

    // --- DRAMA LOGGING ---

    /// <summary>
    /// Logs a specific TV-worthy event using PlayerIdentity data.
    /// </summary>
    public void LogDrama(DramaType type, Transform spot, PlayerIdentity actor, PlayerIdentity victim, int score, string caption, float intensity, Resource transferredResource = null) 
    {
        // Ensure episode-relative timebase is initialized even if the first thing that happens is drama.
        if (!_episodeStartTimeInitialized)
        {
            _episodeStartTime = Time.time;
            _episodeStartTimeInitialized = true;
        }

        // Deduplicate rapid-fire identical events (e.g., repeated steal presses while standing in range).
        int actorIndex = actor != null ? actor.playerIndex : -1;
        int victimIndex = victim != null ? victim.playerIndex : -1;
        string dedupKey = $"{type}:{actorIndex}:{victimIndex}";

        if (_lastDramaTimeByKey.TryGetValue(dedupKey, out float lastTime))
        {
            if (Time.time - lastTime < dramaDedupWindowSeconds)
                return;
        }
        _lastDramaTimeByKey[dedupKey] = Time.time;

        // Fallback to name if the identity is null (for environment events)
        string actorName = actor != null ? actor.name : "Environment";
        string victimName = victim != null ? victim.name : "None";

        float episodeTimestamp = Time.time - _episodeStartTime;

        int actorPlayerIndex = actor != null ? actor.playerIndex : -1;
        int victimPlayerIndex = victim != null ? victim.playerIndex : -1;

        DramaEvent newEvent = new DramaEvent(
            type, 
            episodeTimestamp,
            spot, 
            actorName, 
            victimName, 
            actorPlayerIndex,
            victimPlayerIndex,
            transferredResource,
            score, 
            caption, 
            intensity
        );
    
        dramaRegistry.Add(newEvent);
    
        string debugColor = score >= 0 ? "#00FFFF" : "#FF4500"; // Cyan vs Deep Orange
        string trendIcon = score >= 0 ? "▲" : "▼";
        string intensityStars = new string('★', Mathf.RoundToInt(intensity / 2f)).PadRight(5, '☆');
        string detailedLog = $"<color={debugColor}><b>[TV EVENT]</b></color> " +
                             $"<color=white>[{episodeTimestamp:F2}s]</color> " +
                             $"<b>{actorName}</b> {trendIcon} <b>{type}</b> " +
                             $"{(victim != null ? $"on <b>{victim.name}</b> " : "")}" +
                             $"| <color={debugColor}>Score: {score:+#;-#;0}</color> " +
                             $"| Intensity: <color=yellow>{intensityStars}</color> " +
                             $"\n<color=grey><i>Caption: \"{caption}\"</i></color>";

        Debug.Log(detailedLog);
    }
    
    // Call this at the end of the round or start of a new one
    public void ResetEpisode()
    {
        productionLedger.Clear();
        dramaRegistry.Clear();
        _lastDramaTimeByKey.Clear();
        _episodeStartTimeInitialized = false;
        isRecording = true;
    }
}