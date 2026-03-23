using UnityEngine;
using System.Collections.Generic;

public static class GameResultData
{
    public static int WinnerId;
    public static Color WinnerColor;
    public static string WinnerCharacter;

    // Persisted between gameplay -> playback -> postgame.
    // Key is PlayerIdentity.playerIndex.
    public static readonly Dictionary<int, int> BaseScoresByPlayerIndex = new Dictionary<int, int>();
    public static readonly Dictionary<int, Color> PlayerColorsByIndex = new Dictionary<int, Color>();

    public static void Reset()
    {
        WinnerId = 0;
        WinnerColor = Color.white;
        WinnerCharacter = string.Empty;
        BaseScoresByPlayerIndex.Clear();
        PlayerColorsByIndex.Clear();
    }
}
