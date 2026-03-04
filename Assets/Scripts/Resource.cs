using UnityEngine;

[CreateAssetMenu(fileName = "Resource", menuName = "Scriptable Objects/Resources")]
public class Resource : ScriptableObject
{
    public string resourceName;
    public Sprite resourceIcon;
    public Color tint = Color.white;
}
