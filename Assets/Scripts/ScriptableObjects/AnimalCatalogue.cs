using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalCatalog", menuName = "GameData/Animal Catalog")]
public class AnimalCatalog : ScriptableObject
{
    public List<AnimalDefinition> animals;

    public AnimalDefinition GetRandomAnimal()
    {
        if (animals == null || animals.Count == 0) return null;
        return animals[Random.Range(0, animals.Count)];
    }
}