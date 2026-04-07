using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = CameraProvider.MainCamera;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            // If the camera is still null, try to get it again.
            // This can happen if the Billboard script initializes before the CameraProvider.
            mainCamera = CameraProvider.MainCamera;
            if (mainCamera == null) return;
        }

        transform.forward = mainCamera.transform.forward;
    }
}