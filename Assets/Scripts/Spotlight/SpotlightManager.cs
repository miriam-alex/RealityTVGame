using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpotlightManager : MonoBehaviour
{
    public static SpotlightManager Instance { get; private set; }

    [Header("Active Players")]
    public PlayerRuntimeSet playerRuntimeSet;
    private List<GameObject> activePlayers;

    [Header("Settings")]
    public float interval = 10f;
    public static string spotlightChildName = "Spotlight";

    private GameObject currentSpotlightPlayer;
    private Coroutine spotlightCoroutine;

    void Awake()
    {
        // Safe singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        spotlightCoroutine = StartCoroutine(SpotlightRoutine());
        activePlayers = playerRuntimeSet.Items;
    }

    IEnumerator SpotlightRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            PickRandomSpotlight();
        }
    }

    // 🔥 Force immediate switch AND reset timer
    public void ForcePickNewSpotlight()
    {
        if (spotlightCoroutine != null)
            StopCoroutine(spotlightCoroutine);

        PickRandomSpotlight();

        spotlightCoroutine = StartCoroutine(SpotlightRoutine());
    }

    void PickRandomSpotlight()
    {
        if (activePlayers == null || activePlayers.Count == 0)
        {
            Debug.LogWarning("SpotlightManager: No active players in the list.");
            return;
        }

        // Turn off previous spotlight
        if (currentSpotlightPlayer != null)
            SetSpotlightVisible(currentSpotlightPlayer, false);

        // Avoid selecting same player twice (if possible)
        List<GameObject> possiblePlayers = new List<GameObject>(activePlayers);

        if (currentSpotlightPlayer != null && possiblePlayers.Count > 1)
            possiblePlayers.Remove(currentSpotlightPlayer);

        currentSpotlightPlayer = possiblePlayers[Random.Range(0, possiblePlayers.Count)];

        SetSpotlightVisible(currentSpotlightPlayer, true);
        Debug.Log($"Spotlight on: {currentSpotlightPlayer.name}");
    }

    void SetSpotlightVisible(GameObject player, bool visible)
    {
        Transform spotlightTransform = player.transform.Find(spotlightChildName);

        if (spotlightTransform != null)
            spotlightTransform.gameObject.SetActive(visible);
        else
            Debug.LogWarning($"SpotlightManager: Could not find child '{spotlightChildName}' on {player.name}");

        PlayerIdentity id = player.GetComponent<PlayerIdentity>();
        if (id != null)
            id.spotlightOn = visible;
    }
}