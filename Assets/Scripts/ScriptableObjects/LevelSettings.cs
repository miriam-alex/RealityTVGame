
using UnityEngine;

// If there is a line here like "namespace MyGame {", delete it 
// and remove the corresponding closing brace "}" at the end of the file.

[CreateAssetMenu(fileName = "NewLevelSettings", menuName = "Game/Level Settings")]
public class LevelSettings : ScriptableObject
{
    public string sceneName;
    public float timeLimit = 120f;
    public Vector3[] spawnPoints;
}