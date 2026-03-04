using UnityEngine;

public class TestStation : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered state " + gameObject.name);
        }
    }
}
