using UnityEngine;

/// <summary>
/// A static class to provide a reliable, central reference to the main gameplay camera.
/// This avoids the issues with using Camera.main in scenes with multiple or changing cameras.
/// </summary>
public static class CameraProvider
{
    /// <summary>
    /// The main gameplay camera. This should be set by the GameInitializer.
    /// </summary>
    public static Camera MainCamera { get; set; }
}
