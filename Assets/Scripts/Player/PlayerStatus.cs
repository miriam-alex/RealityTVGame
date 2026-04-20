using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public bool isStunned { get; private set; }
    public int successfulSteals = 0; // Track steals here
    public const int STEAL_LIMIT = 3;

    public void ApplyStun(float duration)
    {
        if (!isStunned) StartCoroutine(StunRoutine(duration));
    }

    // Call this whenever a steal is successful
    public void RegisterSuccessfulSteal()
    {
        successfulSteals++;
        if (successfulSteals > STEAL_LIMIT)
        {
            ApplyStun(5f);
            //successfulSteals = 0; // Reset after punishment
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
}