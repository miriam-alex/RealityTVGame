using UnityEngine;

[CreateAssetMenu(menuName = "Resource/Resource")]
public class Resource : ScriptableObject
{
    public string resourceName;
    public int pointsValue;
    public Color tint = Color.white; // Tint color for the resource
    public GameObject prefab; // Prefab for this resource
    // Add other shared data here
}
