using UnityEngine;
using System.Collections.Generic;

public static class GameResultData
{
    public static int WinnerId;
    public static GameObject WinnerPrefab;
    public static string WinnerCharacter;

    // Persisted between gameplay -> playback -> postgame.
    public static readonly Dictionary<int, int> BaseScoresByPlayerIndex = new Dictionary<int, int>();
    public static readonly Dictionary<int, GameObject> PlayerIndexToPrefab = new Dictionary<int, GameObject>();

    public static void Reset()
    {
        WinnerId = 0;
        WinnerPrefab = null;
        WinnerCharacter = string.Empty;
        BaseScoresByPlayerIndex.Clear();
        PlayerIndexToPrefab.Clear();
    }
}
