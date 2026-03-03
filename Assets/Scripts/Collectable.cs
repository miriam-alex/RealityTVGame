using UnityEngine;

public class Collectable : MonoBehaviour
{
    public int points = 10; // Value of the collectable

    private void OnTriggerEnter(Collider other)
    {
        PlayerIdentity id = other.GetComponent<PlayerIdentity>();
        if (other.CompareTag("Player") && id != null)
        {
            Debug.Log("Collected!");
            FindObjectOfType<ScoreManager>().AddScore(points, id.playerIndex);
            Destroy(gameObject); 
        }
    }
}