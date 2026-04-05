using UnityEngine.InputSystem;
public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput input)
    {
        PlayerIndex = input.playerIndex;
        Input = input;
    }
    public PlayerInput Input { get; set; }
    public int PlayerIndex { get; set; }
    public bool IsReady { get; set; }
}