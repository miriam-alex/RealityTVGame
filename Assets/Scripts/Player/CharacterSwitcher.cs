
using UnityEngine;
using UnityEngine.InputSystem;
public class CharacterSwitcher : MonoBehaviour 
{
    public GameObject[] characterPrefabs; // Assign your 4 character prefabs here

    void Start() {
        int index = GetComponent<PlayerInput>().playerIndex;
        // Spawn the character corresponding to the join order
        Instantiate(characterPrefabs[index], transform.position, transform.rotation, this.transform);
    }
}