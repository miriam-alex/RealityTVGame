using UnityEngine;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public bool isStunned { get; private set; }

    public void ApplyStun(float duration)
    {
        if (!isStunned)
        {
            StartCoroutine(StunRoutine(duration));
        }
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }
}