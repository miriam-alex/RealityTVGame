using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlaybackOrchestrator : MonoBehaviour 
{
    public GameObject ghostPrefab;
    public GameObject scoreBubblePrefab;
    public PlayerRuntimeSet originalRuntimeSet; 

    [Header("UI")]
    [Tooltip("Optional. Drag a TextMeshProUGUI here to show the selected drama event during playback.")]
    public TMP_Text dramaDescriptionText;

    [Header("Clip Visuals")]

    [Tooltip("How long the transfer prop takes to travel.")]
    public float transferTravelSeconds = 1f;

    [Tooltip("World-space offset from the player position for the transfer prop.")]
    public Vector3 transferWorldOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Scene Flow")]
    [Tooltip("Scene to load when the drama clip finishes.")]
    public string postGameSceneName = "PostGame";

    [Header("Drama Clip")]
    [Tooltip("If true and drama events exist, playback will jump to a short clip around the best event.")]
    public bool playDramaClipOnly = true;

    [Tooltip("Total clip duration in seconds.")]
    public float dramaClipDurationSeconds = 5f;

    [Tooltip("How much of the clip occurs before the event timestamp.")]
    public float dramaClipLeadInSeconds = 1.5f;
    
    [Header("Juice Settings")]
    public float zoomedFOV = 30f;
    public float cameraSmoothSpeed = 5f;
    
    [Header("Framing")]
    [Tooltip("How much higher the camera looks to keep the item at the bottom.")]
    public float itemFramingVerticalOffset = 0f;
    

    
    // PRIVATE VARIABLES
    private Camera _playbackCam;
    private float _defaultFOV;
    private bool _isZooming;
    private Dictionary<string, GameObject> _spawnedGhosts = new Dictionary<string, GameObject>();
    private float _playbackTime = 0f;

    private bool _hasClipWindow;
    private float _clipStartTime;
    private float _clipEndTime;
    private List<DramaEvent> _topDramaEvents = new List<DramaEvent>();
    private DramaEvent _selectedDramaEvent;
    private int _currentClipIndex = 0;
    private bool _playedTransferVisual;
    private GameObject _currentTransferInstance;
    

    void Start() 
    {
        if (DirectorManager.Instance == null) return;
        
        _playbackCam = Camera.main;
        if (_playbackCam != null) _defaultFOV = _playbackCam.fieldOfView;

        DirectorManager.Instance.SetRecording(false);

        // Logic to find and sort the Top 3 moments
        ConfigureHighlightReel();

        if (playDramaClipOnly && !_hasClipWindow)
        {
            SceneManager.LoadScene(postGameSceneName);
            return;
        }

        SpawnAllRecordedActors();
        SetupClip(_currentClipIndex);
    }
    
    private void ConfigureHighlightReel()
    {
        List<DramaEvent> allEvents = new List<DramaEvent>(DirectorManager.Instance.dramaRegistry);
        if (allEvents.Count == 0)
        {
            _hasClipWindow = false;
            return;
        }

        // Sort by intensity (Highest first)
        allEvents.Sort((a, b) => b.dramaIntensity.CompareTo(a.dramaIntensity));

        // Take Top 3
        int count = Mathf.Min(3, allEvents.Count);
        for (int i = 0; i < count; i++)
        {
            _topDramaEvents.Add(allEvents[i]);
        }

        _hasClipWindow = true;
    }
    
    private void SetupClip(int index)
    {
        if (index >= _topDramaEvents.Count) return;

        _selectedDramaEvent = _topDramaEvents[index];
    
        float leadIn = Mathf.Clamp(dramaClipLeadInSeconds, 0f, dramaClipDurationSeconds);
        _clipStartTime = Mathf.Max(0f, _selectedDramaEvent.timestamp - leadIn);
        _clipEndTime = _clipStartTime + dramaClipDurationSeconds;
    
        _playbackTime = _clipStartTime;
        _playedTransferVisual = false; // Reset for the new clip
        _hasClipWindow = true;

        UpdateDramaDescriptionUI();
    }
    
    private void TriggerScoreBubble(int score, Transform target)
    {
        if (score == 0 || scoreBubblePrefab == null) return;
        GameObject b = Instantiate(scoreBubblePrefab, target.position + Vector3.up * 2f, Quaternion.identity);
        if (b.TryGetComponent(out ScoreBubble script)) script.Setup(score);
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
            $"{e.tvCaption}\n" +
            $"Score: {e.scoreImpact:+#;-#;0}";
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
        
        // 1. DYNAMIC CAMERA (Includes Player Focus and Item Framing)
        HandleDynamicCamera();

        // 2. CHECK CLIP END / JUMP TO NEXT
        if (_hasClipWindow && _playbackTime > _clipEndTime)
        {
            _currentClipIndex++;
            if (_currentClipIndex < _topDramaEvents.Count)
                SetupClip(_currentClipIndex);
            else
                FinalizeScoresAndTransition();
            return;
        }

        // 3. MOVE GHOSTS
        foreach (var entry in _spawnedGhosts) 
        {
            ApplyFrame(entry.Value.transform, DirectorManager.Instance.productionLedger[entry.Key], _playbackTime);
        }

        // 4. TRIGGER VISUALS (Flash, Bubble, Item)
        TryPlayTransferVisual();
    }
    
    private void HandleDynamicCamera()
    {
        if (_playbackCam == null || _selectedDramaEvent == null) return;

        _isZooming = _playbackTime >= (_selectedDramaEvent.timestamp - 0.5f);
        float targetFOV = _isZooming ? zoomedFOV : _defaultFOV;
        _playbackCam.fieldOfView = Mathf.Lerp(_playbackCam.fieldOfView, targetFOV, Time.deltaTime * cameraSmoothSpeed);

        if (_isZooming) 
        {
            _spawnedGhosts.TryGetValue(_selectedDramaEvent.actorID, out GameObject a);
            _spawnedGhosts.TryGetValue(_selectedDramaEvent.victimID, out GameObject v);
    
            Vector3 lookAtPos = Vector3.zero;

            if (_currentTransferInstance != null)
                lookAtPos = _currentTransferInstance.transform.position + Vector3.up * itemFramingVerticalOffset;
            else if (a != null && v != null)
                lookAtPos = Vector3.Lerp(a.transform.position, v.transform.position, 0.5f) + transferWorldOffset;

            if (lookAtPos != Vector3.zero) 
            {
                Quaternion lookRot = Quaternion.LookRotation(lookAtPos - _playbackCam.transform.position);
                _playbackCam.transform.rotation = Quaternion.Slerp(_playbackCam.transform.rotation, lookRot, Time.deltaTime * cameraSmoothSpeed);
            }
        }
    }

    private void TryPlayTransferVisual()
    {
        if (_playedTransferVisual || _selectedDramaEvent == null) return;
        if (_playbackTime < _selectedDramaEvent.timestamp) return;
        if (_selectedDramaEvent.type != DramaType.GiveItem && _selectedDramaEvent.type != DramaType.StealItem) return;

        _playedTransferVisual = true;

        // Visual 1: Red Flash for Steals
        if (_selectedDramaEvent.type == DramaType.StealItem)
            StartCoroutine(FlashCameraColor(Color.red, 0.4f));

        // Visual 2: Score Bubble over the Actor
        if (_spawnedGhosts.TryGetValue(_selectedDramaEvent.actorID, out GameObject actorGhost))
            TriggerScoreBubble(_selectedDramaEvent.scoreImpact, actorGhost.transform);

        // Visual 3: Item Toss
        int actorIndex, victimIndex;
        TryParsePlayerIndex(_selectedDramaEvent.actorID, out actorIndex);
        TryParsePlayerIndex(_selectedDramaEvent.victimID, out victimIndex);

        int fromIdx = _selectedDramaEvent.type == DramaType.StealItem ? victimIndex : actorIndex;
        int toIdx = _selectedDramaEvent.type == DramaType.StealItem ? actorIndex : victimIndex;

        if (_spawnedGhosts.TryGetValue($"Player_{fromIdx}", out GameObject fromG) && 
            _spawnedGhosts.TryGetValue($"Player_{toIdx}", out GameObject toG))
        {
            GameObject prefab = _selectedDramaEvent.transferredResource?.prefab;
            if (prefab != null) StartCoroutine(PlayTransferProp(prefab, fromG.transform, toG.transform));
        }
    }

    private IEnumerator PlayTransferProp(GameObject prefab, Transform from, Transform to)
    {
        _currentTransferInstance = Instantiate(prefab);
        if (_currentTransferInstance.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        float t = 0f;
        Vector3 start = from.position + transferWorldOffset;
        Vector3 end = to.position + transferWorldOffset;

        while (t < transferTravelSeconds)
        {
            t += Time.deltaTime;
            _currentTransferInstance.transform.position = Vector3.Lerp(start, end, t / transferTravelSeconds);
            yield return null;
        }

        Destroy(_currentTransferInstance);
        _currentTransferInstance = null;
    }
    
    private IEnumerator FlashCameraColor(Color col, float dur)
    {
        if (_playbackCam == null) yield break;
        _playbackCam.clearFlags = CameraClearFlags.SolidColor;
        Color orig = _playbackCam.backgroundColor;
        float elapsed = 0;
        while(elapsed < dur) {
            elapsed += Time.deltaTime;
            _playbackCam.backgroundColor = Color.Lerp(col, orig, elapsed/dur);
            yield return null;
        }
        _playbackCam.clearFlags = CameraClearFlags.Skybox; // Or original
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