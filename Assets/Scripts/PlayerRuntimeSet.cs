using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerSet", menuName = "GameData/PlayerSet")]
public class PlayerRuntimeSet : ScriptableObject 
{
    // We use GameObject to be safe and versatile
    public List<GameObject> Items = new List<GameObject>();

    // This ensures the list starts fresh every time you press Play
    private void OnEnable() => Items.Clear();

    public void Add(GameObject obj)
    {
        if (obj != null && !Items.Contains(obj)) 
        {
            Items.Add(obj);
        }
    }

    public void Remove(GameObject obj)
    {
        if (Items.Contains(obj)) 
        {
            Items.Remove(obj);
        }
    }
}