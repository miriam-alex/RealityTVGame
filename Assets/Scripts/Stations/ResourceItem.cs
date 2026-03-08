using UnityEngine;

public class ResourceItem : MonoBehaviour
{
    public Resource resource; // Reference to the definition

    public string ResourceName => resource != null ? resource.resourceName : "";
    public int PointsValue => resource != null ? resource.pointsValue : 0;
    public Color Tint => resource != null ? resource.tint : Color.white;

    private void Start()
    {
        // Try to tint the item's SpriteRenderer if it exists
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Tint;
        }
    }
    // Add other properties as needed
}
