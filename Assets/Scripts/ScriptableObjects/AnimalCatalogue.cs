using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimalCatalog", menuName = "GameData/Animal Catalog")]
public class AnimalCatalog : ScriptableObject
{
    public List<AnimalDefinition> animals;
    
    public AnimalDefinition GetAnimalDefinition(string animalID)
    {
        AnimalDefinition definition = animals.Find(a => a.id == animalID);
        return definition;
    }

    public AnimalDefinition GetAnimalDefinition(int animalIndex)
    {
        return animals[animalIndex];
    }
}