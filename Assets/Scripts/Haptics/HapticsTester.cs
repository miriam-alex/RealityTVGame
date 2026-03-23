using UnityEngine;

public class HapticsTester : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HapticsManager.Instance?.Pulse(0.7f, 0.2f);
        }
    }
}
