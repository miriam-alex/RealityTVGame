using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlaybackOrchestrator : MonoBehaviour 
{
    public GameObject ghostPrefab;
    public PlayerRuntimeSet originalRuntimeSet; 

    [Header("UI")]
    [Tooltip("Optional. Drag a TextMeshProUGUI here to show the selected drama event during playback.")]
    public TMP_Text dramaDescriptionText;

    [Header("Clip Visuals")]
    [Tooltip("Optional. Drag a prefab here to visualize Give/Steal as an item moving between players during the clip.")]
    public GameObject transferItemPrefab;

    [Tooltip("How long the transfer prop takes to travel.")]
    public float transferTravelSeconds = 0.6f;

    [Tooltip("World-space offset from the player position for the transfer prop.")]
    public Vector3 transferWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Scene Flow")]
    [Tooltip("Scene to load when the drama clip finishes.")]
    public string postGameSceneName = "PostGame";

    [Header("Drama Clip")]
    [Tooltip("If true and drama events exist, playback will jump to a short clip around the best event.")]
    public bool playDramaClipOnly = true;

    [Tooltip("Total clip duration in seconds.")]
    public float dramaClipDurationSeconds = 3f;

    [Tooltip("How much of the clip occurs before the event timestamp.")]
    public float dramaClipLeadInSeconds = 1.5f;
    
    private Dictionary<string, GameObject> _spawnedGhosts = new Dictionary<string, GameObject>();
    private float _playbackTime = 0f;

    private bool _hasClipWindow;
    private float _clipStartTime;
    private float _clipEndTime;

    private DramaEvent _selectedDramaEvent;
    private bool _playedTransferVisual;

    void Start() 
    {
        if (DirectorManager.Instance == null)
        {
            Debug.LogError("[Playback] FAILED: No DirectorManager found in scene!");
            return;
        }

        // Prevent playback ghosts from being recorded back into the ledger.
        DirectorManager.Instance.SetRecording(false);

        Debug.Log($"[Playback] Starting. Found {DirectorManager.Instance.productionLedger.Count} tracks in Ledger.");

        // Decide whether we should play anything BEFORE spawning ghosts.
        ConfigureClipWindowFromDrama();

        if (playDramaClipOnly && !_hasClipWindow)
        {
            // In clip-only mode, no drama means no playback.
            if (dramaDescriptionText != null)
                dramaDescriptionText.text = string.Empty;

            Debug.Log("[Playback] No drama events found; clip-only playback disabled. Nothing will play.");
            enabled = false;
            return;
        }

        SpawnAllRecordedActors();
    }

    private void ConfigureClipWindowFromDrama()
    {
        if (!playDramaClipOnly) return;
        if (DirectorManager.Instance == null) return;

        List<DramaEvent> events = DirectorManager.Instance.dramaRegistry;
        if (events == null || events.Count == 0)
        {
            _hasClipWindow = false;
            _selectedDramaEvent = null;
            UpdateDramaDescriptionUI();
            return;
        }

        DramaEvent best = events[0];
        for (int i = 1; i < events.Count; i++)
        {
            // Prefer higher intensity; tiebreaker: bigger absolute score impact; then later timestamp.
            DramaEvent candidate = events[i];
            if (candidate.dramaIntensity > best.dramaIntensity)
                best = candidate;
            else if (Mathf.Approximately(candidate.dramaIntensity, best.dramaIntensity))
            {
                if (Mathf.Abs(candidate.scoreImpact) > Mathf.Abs(best.scoreImpact))
                    best = candidate;
                else if (Mathf.Abs(candidate.scoreImpact) == Mathf.Abs(best.scoreImpact) && candidate.timestamp > best.timestamp)
                    best = candidate;
            }
        }

        float leadIn = Mathf.Clamp(dramaClipLeadInSeconds, 0f, dramaClipDurationSeconds);
        float clipStart = Mathf.Max(0f, best.timestamp - leadIn);
        float clipEnd = clipStart + Mathf.Max(0.1f, dramaClipDurationSeconds);

        _clipStartTime = clipStart;
        _clipEndTime = clipEnd;
        _hasClipWindow = true;

        _selectedDramaEvent = best;

        _playbackTime = _clipStartTime;

        Debug.Log($"[Playback] Drama clip: '{best.type}' @ {best.timestamp:F2}s | Window [{_clipStartTime:F2}, {_clipEndTime:F2}] ({dramaClipDurationSeconds:F2}s)");

        UpdateDramaDescriptionUI();
    }

    private void UpdateDramaDescriptionUI()
    {
        if (dramaDescriptionText == null) return;
        if (_selectedDramaEvent == null)
        {
            dramaDescriptionText.text = string.Empty;
            return;
        }

        // Keep this plain so it works with any UI style.
        DramaEvent e = _selectedDramaEvent;
        string participants = string.IsNullOrEmpty(e.victimID) || e.victimID == "None"
            ? e.actorID
            : $"{e.actorID} → {e.victimID}";

        string itemLine = e.transferredResource != null ? $"Item: {e.transferredResource.resourceName}\n" : string.Empty;

        dramaDescriptionText.text =
            $"{e.type} ({participants})\n" +
            itemLine +
            $"{e.tvCaption}\n" +
            $"Score: {e.scoreImpact:+#;-#;0} | Intensity: {e.dramaIntensity:0.#} | t={e.timestamp:0.00}s";
    }

    void SpawnAllRecordedActors() 
    {
        foreach (var entry in DirectorManager.Instance.productionLedger)
        {
            string actorId = entry.Key;
            ActorTrack track = entry.Value;

            // Spawn ghosts only for player tracks.
            bool isPlayerTrack = track.actorType == RecordedActorType.Player;

            // Back-compat heuristic if older recordings didn't set actorType.
            if (!isPlayerTrack && track.actorType == RecordedActorType.Unknown)
                isPlayerTrack = actorId.StartsWith("Player");

            if (!isPlayerTrack)
            {
                Debug.Log($"[Playback] Skip track '{actorId}' type={track.actorType} frames={track.frames.Count}");
                continue;
            }

            Debug.Log($"[Playback] Use track '{actorId}' type={track.actorType} frames={track.frames.Count} hasColor={track.hasColor}");

            Debug.Log($"[Playback] Spawning ghost for: {actorId}");
            
            GameObject ghost = Instantiate(ghostPrefab);
            ghost.name = actorId;

            // Ensure transform-driven playback isn't fighting physics.
            if (ghost.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            ApplyRecordedAppearance(ghost, actorId, track);
            _spawnedGhosts.Add(actorId, ghost);
        }
        
        Debug.Log($"[Playback] Spawning complete. Total ghosts: {_spawnedGhosts.Count}");
    }

    void ApplyRecordedAppearance(GameObject ghost, string actorId, ActorTrack track)
    {
        if (track != null && track.hasColor)
        {
            Transform body = ghost.transform.Find("Player Body/Body");
            if (body != null)
            {
                var renderer = body.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.material.color = track.actorColor;
                    return;
                }
            }

            Debug.LogWarning($"[Playback] {actorId} has recorded color but ghost body mesh was not found.");
        }

        // Fallback: try to pull appearance from a runtime set, if it survived scene load.
        if (originalRuntimeSet == null || originalRuntimeSet.Items.Count == 0)
        {
            Debug.LogWarning($"[Playback] {actorId} is grey because no recorded color and RuntimeSet is null/empty.");
            return;
        }
        
        bool foundColor = false;
        foreach (GameObject original in originalRuntimeSet.Items)
        {
            if (original == null) continue;

            // Prefer matching by stable player index if possible.
            if (original.TryGetComponent(out PlayerIdentity originalIdentity))
            {
                string expectedId = $"Player_{originalIdentity.playerIndex}";
                if (expectedId != actorId)
                    continue;

                Transform body = ghost.transform.Find("Player Body/Body");
                if (body != null)
                {
                    var renderer = body.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = originalIdentity.color;
                        Debug.Log($"[Playback] Successfully colored {actorId} to {originalIdentity.color} (fallback)");
                        foundColor = true;
                        break;
                    }
                }
                else Debug.LogError($"[Playback] Found color for {actorId} but 'Player Body/Body' child missing!");
            }
        }

        if (!foundColor) Debug.LogWarning($"[Playback] No matching identity found in RuntimeSet for {actorId}");
    }

    void Update() 
    {
        _playbackTime += Time.deltaTime;

        if (_hasClipWindow && _playbackTime > _clipEndTime)
        {
            FinalizeScoresAndTransition();
            return;
        }

        foreach (var entry in _spawnedGhosts) 
        {
            string name = entry.Key;
            Transform ghostTransform = entry.Value.transform;
            ActorTrack track = DirectorManager.Instance.productionLedger[name];

            ApplyFrame(ghostTransform, track, _playbackTime);
        }

        TryPlayTransferVisual();

        // Optional: Log the first few seconds of playback to see if time is moving
        if (Time.frameCount % 60 == 0) // Roughly once per second
        {
            foreach (var ghost in _spawnedGhosts.Values)
            {
                Debug.Log($"[Playback] Time: {_playbackTime:F2}s | Sample Ghost Pos: {ghost.transform.position}");
                break;
            }
        }
    }

    private void TryPlayTransferVisual()
    {
        if (_playedTransferVisual) return;
        if (transferItemPrefab == null && (_selectedDramaEvent == null || _selectedDramaEvent.transferredResource == null || _selectedDramaEvent.transferredResource.prefab == null))
            return;
        if (_selectedDramaEvent == null) return;
        if (!_hasClipWindow) return;

        // Only for Give/Steal, and only once per clip.
        if (_selectedDramaEvent.type != DramaType.GiveItem && _selectedDramaEvent.type != DramaType.StealItem)
            return;

        if (_playbackTime < _selectedDramaEvent.timestamp)
            return;

        int actorIndex = _selectedDramaEvent.actorIndex;
        int victimIndex = _selectedDramaEvent.victimIndex;

        // Fallback parse from ids if indices are missing.
        if (actorIndex < 0 && !TryParsePlayerIndex(_selectedDramaEvent.actorID, out actorIndex))
            return;
        if (victimIndex < 0 && !TryParsePlayerIndex(_selectedDramaEvent.victimID, out victimIndex))
            return;

        // Determine direction.
        // GiveItem: actor -> victim.  StealItem: victim -> actor.
        int fromIndex = _selectedDramaEvent.type == DramaType.StealItem ? victimIndex : actorIndex;
        int toIndex = _selectedDramaEvent.type == DramaType.StealItem ? actorIndex : victimIndex;

        if (!_spawnedGhosts.TryGetValue($"Player_{fromIndex}", out GameObject fromGhost) || fromGhost == null)
            return;
        if (!_spawnedGhosts.TryGetValue($"Player_{toIndex}", out GameObject toGhost) || toGhost == null)
            return;

        GameObject prefabToUse = _selectedDramaEvent.transferredResource != null && _selectedDramaEvent.transferredResource.prefab != null
            ? _selectedDramaEvent.transferredResource.prefab
            : transferItemPrefab;

        if (prefabToUse == null)
            return;

        _playedTransferVisual = true;
        StartCoroutine(PlayTransferProp(prefabToUse, fromGhost.transform, toGhost.transform));
    }

    private IEnumerator PlayTransferProp(GameObject prefab, Transform from, Transform to)
    {
        GameObject prop = Instantiate(prefab);

        // Keep it purely visual.
        if (prop.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        float duration = Mathf.Max(0.05f, transferTravelSeconds);
        float t = 0f;

        Vector3 start = from.position + transferWorldOffset;
        Vector3 end = to.position + transferWorldOffset;

        prop.transform.position = start;

        while (t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            prop.transform.position = Vector3.Lerp(start, end, a);
            yield return null;
        }

        prop.transform.position = end;
        Destroy(prop);
    }

    private void FinalizeScoresAndTransition()
    {
        // Avoid double-fire.
        enabled = false;

        ApplyClipScoreAdjustments();
        DetermineWinnerAndStoreResult();

        if (string.IsNullOrWhiteSpace(postGameSceneName))
        {
            Debug.LogError("[Playback] postGameSceneName is empty; cannot transition.");
            return;
        }

        Debug.Log($"[Playback] Drama clip complete. Loading '{postGameSceneName}'.");
        SceneManager.LoadScene(postGameSceneName);
    }

    private void ApplyClipScoreAdjustments()
    {
        if (!_hasClipWindow) return;
        if (DirectorManager.Instance == null) return;
        var events = DirectorManager.Instance.dramaRegistry;
        if (events == null || events.Count == 0) return;

        // Ensure we have score entries for known players.
        foreach (var kvp in DirectorManager.Instance.productionLedger)
        {
            if (kvp.Value == null || kvp.Value.actorType != RecordedActorType.Player) continue;
            if (TryParsePlayerIndex(kvp.Key, out int pIndex) && !GameResultData.BaseScoresByPlayerIndex.ContainsKey(pIndex))
                GameResultData.BaseScoresByPlayerIndex[pIndex] = 0;
        }

        // Apply score impacts for any drama events that occur during the clip window.
        for (int i = 0; i < events.Count; i++)
        {
            DramaEvent e = events[i];
            if (e == null) continue;

            if (e.timestamp < _clipStartTime || e.timestamp > _clipEndTime)
                continue;

            int actorIndex = e.actorIndex;
            if (actorIndex < 0)
            {
                // Fallback parse from actorID string if needed.
                if (!TryParsePlayerIndex(e.actorID, out actorIndex))
                    continue;
            }

            if (!GameResultData.BaseScoresByPlayerIndex.ContainsKey(actorIndex))
                GameResultData.BaseScoresByPlayerIndex[actorIndex] = 0;

            GameResultData.BaseScoresByPlayerIndex[actorIndex] += e.scoreImpact;
            GameResultData.BaseScoresByPlayerIndex[actorIndex] = Mathf.Max(0, GameResultData.BaseScoresByPlayerIndex[actorIndex]);
        }
    }

    private void DetermineWinnerAndStoreResult()
    {
        int winnerPlayerIndex = Timer.DetermineWinnerId(GameResultData.BaseScoresByPlayerIndex);
        GameResultData.WinnerId = winnerPlayerIndex;

        // Prefer color captured from gameplay; otherwise fall back to recorded track color.
        if (GameResultData.PlayerColorsByIndex.TryGetValue(winnerPlayerIndex, out Color c))
        {
            GameResultData.WinnerColor = c;
            return;
        }

        if (DirectorManager.Instance != null)
        {
            string key = $"Player_{winnerPlayerIndex}";
            if (DirectorManager.Instance.productionLedger.TryGetValue(key, out ActorTrack track) && track != null && track.hasColor)
            {
                GameResultData.WinnerColor = track.actorColor;
                return;
            }
        }

        GameResultData.WinnerColor = Color.white;
    }

    private bool TryParsePlayerIndex(string id, out int playerIndex)
    {
        playerIndex = -1;
        if (string.IsNullOrEmpty(id)) return false;

        // Expected formats: "Player_0" or gameplay names like "Player 1" / "Player 2".
        if (id.StartsWith("Player_"))
        {
            string n = id.Substring("Player_".Length);
            return int.TryParse(n, out playerIndex);
        }

        if (id.StartsWith("Player "))
        {
            string n = id.Substring("Player ".Length);
            if (int.TryParse(n, out int oneBased))
            {
                playerIndex = Mathf.Max(0, oneBased - 1);
                return true;
            }
        }

        return false;
    }

    void ApplyFrame(Transform t, ActorTrack track, float time)
    {
        if (track.frames.Count == 0) return;

        // Check if our current playback time is even within the recorded range
        if (time < track.frames[0].timestamp) return;

        for (int i = 0; i < track.frames.Count; i++)
        {
            if (track.frames[i].timestamp >= time)
            {
                t.position = track.frames[i].position;
                t.rotation = track.frames[i].rotation;
                return; // Found it, stop searching
            }
        }
    }
}