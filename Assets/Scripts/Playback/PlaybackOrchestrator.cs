using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlaybackOrchestrator : MonoBehaviour 
{
    public AnimalCatalog animalCatalog;
    
    [Header("UI")]
    [Tooltip("Optional. Drag a TextMeshProUGUI here to show the selected drama event during playback.")]
    public TMP_Text dramaDescriptionText;

    [Header("Clip Visuals")]

    [Tooltip("How long the transfer prop takes to travel when GIVING.")]
    public float giveTravelSeconds = 1f;
    
    [Tooltip("How long the transfer prop takes to travel when STEALING.")]
    public float stealTravelSeconds = 0.5f;

    [Header("Scene Flow")]
    [Tooltip("Scene to load when the drama clip finishes.")]
    public string postGameSceneName = "PostGame";

    [Header("Drama Clip")]
    public int maxNumberOfDramaClips = 3;
    
    [Tooltip("If true and drama events exist, playback will jump to a short clip around the best event.")]
    public bool playDramaClipOnly = true;

    [Tooltip("Total clip duration in seconds.")]
    public float dramaClipDurationSeconds = 5f;

    [Tooltip("How much of the clip occurs before the event timestamp.")]
    public float dramaClipLeadInSeconds = 0f;
    
    [Header("Juice Settings")]
    public float zoomedFOV = 30f;
    public float cameraSmoothSpeed = 5f;
    
    [Header("Framing")]
    [Tooltip("How much higher the camera looks to keep the item at the bottom.")]
    public float itemFramingVerticalOffset = 0f;

    [SerializeField] private Light spotlight1;
    [SerializeField] private Light spotlight2;
    
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
        DirectorManager.Instance.SetPlayerInput(false);
        
        FindAnyObjectByType<PlayerStationManager>()?.SpawnStationsForPlayers(); // we want the station prefab 

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

        ShuffleList(allEvents);

        int count = Mathf.Min(maxNumberOfDramaClips, allEvents.Count);
        for (int i = 0; i < count; i++)
        {
            _topDramaEvents.Add(allEvents[i]);
        }

        _hasClipWindow = true;
    }
    
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    
    private void SetupClip(int index)
    {
        if (index >= _topDramaEvents.Count) return;

        _selectedDramaEvent = _topDramaEvents[index];
    
        float leadIn = Mathf.Clamp(dramaClipLeadInSeconds, 0f, dramaClipDurationSeconds);
        _clipStartTime = Mathf.Max(0f, _selectedDramaEvent.timestamp - leadIn);
        _clipEndTime = _clipStartTime + dramaClipDurationSeconds;
    
        _playbackTime = _clipStartTime;
        _playedTransferVisual = false;
        _hasClipWindow = true;

        UpdateDramaDescriptionUI();
        
        // we want to only apply the first frame of each new scene
        foreach (var entry in _spawnedGhosts) 
        {
            ApplyFrame(entry.Value.transform, DirectorManager.Instance.productionLedger[entry.Key], _playbackTime);
        }
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

        string caption = GenerateTVCaption(e.actorIndex, e.victimIndex, e.type);
        
        dramaDescriptionText.text = $"{caption} Impact: {e.scoreImpact:+#;-#;0}k followers.";
    }

    private string GenerateTVCaption(int actorIndex, int subjectIndex, DramaType type)
    {
        string actorName = animalCatalog.animals[actorIndex].id;
        string subjectName = animalCatalog.animals[subjectIndex].id;

        if (type == DramaType.StealItem)
        {
            return $"{actorName} stole from {subjectName}!";
        }
        else if (type == DramaType.GiveItem)
        {
            return $"{actorName} is a saint, giving to {subjectName}!";
        }

        return null;
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
            
            GameObject playbackActor = SpawnPlaybackActor(actorId);
            _spawnedGhosts.Add(actorId, playbackActor);
        }
        
        Debug.Log($"[Playback] Spawning complete. Total ghosts: {_spawnedGhosts.Count}");
    }

    private GameObject SpawnPlaybackActor(string actorId)
    {
        GameObject root = new GameObject(actorId);

        GameObject visualPrefab = ResolvePlaybackVisualPrefab(actorId);
        if (visualPrefab != null)
        {
            GameObject visual = Instantiate(visualPrefab, root.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"[Playback] No prefab found for '{actorId}'. Assign bodyPrefab on PlayerIdentity.");
        }

        // Ensure transform-driven playback isn't fighting physics on any spawned prefab.
        Rigidbody[] rigidbodies = root.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
            rigidbodies[i].useGravity = false;
        }

        return root;
    }

    private GameObject ResolvePlaybackVisualPrefab(string actorId)
    {
        // 1. Parse the player index (e.g., "Player_0" -> 0)
        if (TryParsePlayerIndex(actorId, out int playerIndex))
        {
            // 2. Check if we have a recorded animal ID for this player index
            if (GameResultData.PlayerIndexToAnimalId.TryGetValue(playerIndex, out string animalId))
            {
                // 3. Resolve the string ID to an actual prefab via the catalog
                if (animalCatalog != null)
                {
                    AnimalDefinition definition = animalCatalog.animals.Find(a => a.id == animalId);
                    if (definition != null && definition.prefab != null)
                    {
                        Debug.Log($"[Playback] Resolved {actorId} as animal ID: {animalId}");
                        return definition.prefab;
                    }
                    else
                    {
                        Debug.LogError($"[Playback] Catalog contains no prefab for ID: {animalId}");
                    }
                }
                else
                {
                    Debug.LogError("[Playback] AnimalCatalog is missing on PlaybackOrchestrator!");
                }
            }
            else
            {
                Debug.LogWarning($"[Playback] No entry found in GameResultData for PlayerIndex: {playerIndex}");
            }
        }

        return null;
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
                lookAtPos = Vector3.Lerp(a.transform.position, v.transform.position, 0.5f);

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

        int actorIndex, victimIndex;
        TryParsePlayerIndex(_selectedDramaEvent.actorID, out actorIndex);
        TryParsePlayerIndex(_selectedDramaEvent.victimID, out victimIndex);

        int fromIdx = _selectedDramaEvent.type == DramaType.StealItem ? victimIndex : actorIndex;
        int toIdx = _selectedDramaEvent.type == DramaType.StealItem ? actorIndex : victimIndex;

        if (_spawnedGhosts.TryGetValue($"Player_{fromIdx}", out GameObject fromG) && 
            _spawnedGhosts.TryGetValue($"Player_{toIdx}", out GameObject toG))
        {
            SetupTransferSpotlights(fromG, toG, _selectedDramaEvent.type);
            GameObject prefab = _selectedDramaEvent.transferredResource?.prefab;

            Transform fromGTransform = GetCarryPoint(fromG);
            Transform toGTransform = GetCarryPoint(toG);
            if (prefab != null) StartCoroutine(PlayTransferProp(prefab, fromGTransform, toGTransform, _selectedDramaEvent.type));
        }
    }
    
    private void SetupTransferSpotlights(GameObject fromG, GameObject toG, DramaType type)
    {
        // Define colors based on the drama
        Color actorColor = (type == DramaType.GiveItem) ? Color.cyan : Color.yellow;
        Color victimColor = (type == DramaType.StealItem) ? Color.yellow : Color.red;

        // Assign positions and colors
        // We use the 'from' and 'to' logic to decide who gets which color
        bool isSteal = type == DramaType.StealItem;
    
        ConfigureLight(spotlight1, fromG.transform, isSteal ? victimColor : actorColor);
        ConfigureLight(spotlight2, toG.transform, isSteal ? actorColor : victimColor);
    }

    private void ConfigureLight(Light light, Transform target, Color color)
    {
        if (light == null) return;

        // Position the light above the ghost (adjust the height offset as needed)
        light.transform.position = target.position + Vector3.up * 5f;
        light.transform.LookAt(target.position);
    
        light.color = color;
        light.enabled = true;

        // Optional: If you want them to turn off automatically after a delay
        StartCoroutine(DisableLightAfterDelay(light, 2.0f));
    }

    private IEnumerator DisableLightAfterDelay(Light light, float delay)
    {
        yield return new WaitForSeconds(delay);
        light.enabled = false;
    }
    private Transform GetCarryPoint(GameObject playerObject)
    {
        // gameobject must be the animal prefab type
        // good coding practice would prob be to enforce this
        Transform animalTransform = playerObject.transform.GetChild(0);
        Transform carryPoint = animalTransform.Find("ObjectCarryPoint");
        if (carryPoint == null) {
            Debug.LogError("CARRY POINT CANNOT BE FOUND");
        }

        return carryPoint;
    }

    private IEnumerator PlayTransferProp(GameObject prefab, Transform from, Transform to, DramaType dramaType)
    {
        float transferTravelSeconds = (dramaType == DramaType.GiveItem) ? giveTravelSeconds : stealTravelSeconds;
        _currentTransferInstance = Instantiate(prefab);
        if (_currentTransferInstance.TryGetComponent(out Rigidbody rb)) rb.isKinematic = true;

        float t = 0f;
        Vector3 start = from.position;
        Vector3 end = to.position;

        while (t < transferTravelSeconds)
        {
            t += Time.deltaTime;
            _currentTransferInstance.transform.position = Vector3.Lerp(start, end, t / transferTravelSeconds);
            yield return null;
        }

        Destroy(_currentTransferInstance);
        _currentTransferInstance = null;
    }
   
    private void FinalizeScoresAndTransition()
    {
        // Avoid double-fire.
        enabled = false;

        DirectorManager.Instance.SetPlayerInput(true);
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