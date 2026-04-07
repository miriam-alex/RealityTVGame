using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class PlayerConfigManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs = new List<PlayerConfiguration>();
    public static PlayerConfigManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    public void SetPlayerConfiguration(PlayerInput input)
    {
        playerConfigs.Add(new PlayerConfiguration(input));
    }
    
    public List<PlayerConfiguration> GetPlayerConfigs() => playerConfigs;

    public void ReadyPlayer(int index, bool ready)
    {
        var config = playerConfigs.Find(p => p.PlayerIndex == index);
        if (config != null)
        {
            config.IsReady = ready;
        }

        // Check if all joined players are ready to start the game
        CheckForAllReady();
    }

    private void CheckForAllReady()
    {
        // Example: Require at least 2 players and all must be ready
        if (playerConfigs.Count >= 2 && playerConfigs.TrueForAll(p => p.IsReady))
        {
            Debug.Log("All Players Ready! Loading PlayerLobby...");
            UnityEngine.SceneManagement.SceneManager.LoadScene("PlayerLobby");
        }
    }

    public void HandlePlayerReady()
    {
        // Find the first player who is not ready and mark them as ready.
        var config = playerConfigs.Find(p => !p.IsReady);
        if (config != null)
        {
            config.IsReady = true;
            CheckForAllReady();
        }
    }
}