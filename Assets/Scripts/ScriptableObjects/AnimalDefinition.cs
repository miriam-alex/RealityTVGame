using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimal", menuName = "GameData/Animal Definition")]
public class AnimalDefinition : ScriptableObject
{
    public string id;
    public GameObject prefab;
}