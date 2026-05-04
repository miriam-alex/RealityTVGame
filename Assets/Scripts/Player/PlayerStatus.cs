using UnityEngine;
using System.Collections;
public class PlayerStatus : MonoBehaviour
{
    public float nextStealTime;
    public float nextYodelTime;
    public const float COOLDOWN_DURATION = 30.0f;

    public void SetStealCooldown()
    {
        nextStealTime = Time.time + COOLDOWN_DURATION;
    }

    public bool IsOnCooldown()
    {
        return Time.time < nextStealTime;
    }

    public void SetYodelCooldown()
    {
        nextYodelTime = Time.time + COOLDOWN_DURATION;
    }

    public bool IsYodelOnCooldown()
    {
        return Time.time < nextYodelTime;
    }
}