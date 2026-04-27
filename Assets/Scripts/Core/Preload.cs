using UnityEngine;

public class Preload : MonoBehaviour
{
    void Awake()
    {
        // Check if the Managers object already exists in the scene
        if (FindObjectOfType<GameInitializer>() == null)
        {
            // If not, load and instantiate it from the Resources folder
            GameObject managersPrefab = Resources.Load("Managers") as GameObject;
            if (managersPrefab != null)
            {
                Instantiate(managersPrefab);
            }
            else
            {
                Debug.LogError("Managers prefab not found in Resources. Please ensure you have a prefab named 'Managers' in a 'Resources' folder.");
            }
        }
    }
}
