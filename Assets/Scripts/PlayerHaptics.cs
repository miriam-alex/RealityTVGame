using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class PlayerHaptics : MonoBehaviour
{
    private PlayerInput _playerInput;
    private Gamepad _gamepad;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        if (_playerInput != null && _playerInput.devices.Count > 0)
        {
            // to be able to recognize the different controllers and vibrate them individually
            foreach (var device in _playerInput.devices)
            {
                if (device is Gamepad g)
                {
                    _gamepad = g;
                    break;
                }
            }
            if (_gamepad == null)
                _gamepad = _playerInput.devices[0] as Gamepad;
        }
    }
    
    public void Pulse(float duration, float strength)
    {
        if (_gamepad == null)
        {
            return;
        }
        
        _gamepad.SetMotorSpeeds(strength, strength);
        CancelInvoke(nameof(Stop));
        Invoke(nameof(Stop), duration);
    }

    private void Stop()
    {
        if (_gamepad != null)
        {
            _gamepad?.SetMotorSpeeds(0f, 0f);
        }
    }
}
