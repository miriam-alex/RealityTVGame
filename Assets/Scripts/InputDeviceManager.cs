using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InputDeviceManager : MonoBehaviour
{
    public PlayerInput player1Input;
    public PlayerInput player2Input;

    void Start()
    {
        // Increased delay slightly to ensure Unity's InputSystem is fully awake
        StartCoroutine(AssignDevicesWithRetry());
    }

    private IEnumerator AssignDevicesWithRetry()
    {
        yield return new WaitForSeconds(0.5f);
        
        // Filter out "Phantom" devices by checking if they are currently 'added'
        var gamepads = Gamepad.all.Where(g => g.added).ToList();
        Debug.Log($"[InputManager] Detected {gamepads.Count} active gamepads.");

        // Setup P1
        ConfigurePlayer(player1Input, 0, "Player 1", gamepads);
        
        // Setup P2
        ConfigurePlayer(player2Input, 1, "Player 2", gamepads);
    }

    private void ConfigurePlayer(PlayerInput pInput, int gamepadIndex, string pName, List<Gamepad> availableGamepads)
    {
        if (pInput == null) return;

        // Ensure the User is initialized
        if (!pInput.user.valid) pInput.ActivateInput();

        // STRICT ISOLATION
        if (pName == "Player 3" && availableGamepads.Count > 0)
        {
            // LOCK PLAYER 3 TO THE CONTROLLER
            Gamepad controller = availableGamepads[0];
            InputUser.PerformPairingWithDevice(controller, pInput.user);
            
            if (pInput.user.valid && pInput.user.index != InputUser.InvalidId)
            {
                pInput.user.UnpairDevices(); // Wipe "Listen to All"
                InputUser.PerformPairingWithDevice(controller, pInput.user); 
            }
            pInput.neverAutoSwitchControlSchemes = true;
            Debug.Log("Player 3 locked to Gamepad.");
        }
        else if (pName == "Player 1" || pName == "Player 2")
        {
            // Allow Player 1 & 2 to use keyboard or gamepad
            // The PlayerInput component will handle device assignment
        }
    }
}