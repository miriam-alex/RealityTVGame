using UnityEngine;
using System.Collections;
public class PlayerStatus : MonoBehaviour
{
    public float nextStealTime;
    public const float COOLDOWN_DURATION = 5f;

    public void SetStealCooldown()
    {
        nextStealTime = Time.time + COOLDOWN_DURATION;
    }

    public bool IsOnCooldown()
    {
        return Time.time < nextStealTime;
    }

    
    
}