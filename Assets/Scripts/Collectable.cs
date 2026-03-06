using UnityEngine;

public class Collectable : MonoBehaviour
{
    public int points = 10; // Value of the collectable

    private void OnTriggerEnter(Collider other)
    {
        GameObject player = other.gameObject;
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collected!");
            FindAnyObjectByType<ScoreManager>().AddScore(points, player);
            Destroy(gameObject); 
        }
    }
}