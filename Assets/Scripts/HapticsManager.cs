using UnityEngine;
using UnityEngine.InputSystem;

public class HapticsManager : MonoBehaviour
{
    // haptics singleton
    public static HapticsManager Instance;

    // destroys other haptics manager objects
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    // if a controller exists, pulse
    public void Pulse(float duration, float strength)
    {
        if (Gamepad.current == null)
        {
            return;
        }
        
        Gamepad.current.SetMotorSpeeds(strength, strength);
        CancelInvoke(nameof(Stop));
        Invoke(nameof(Stop), duration);
    }

    private void Stop()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
