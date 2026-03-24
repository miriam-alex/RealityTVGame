using UnityEngine;
using System.Collections;

public class RealityActor : MonoBehaviour
{
    private Coroutine _registerRoutine;
    private bool _isRegistered;

    private void OnEnable()
    {
        TryRegisterNowOrLater();
    }

    private void Start()
    {
        // Extra safety: Start runs after all Awakes in the scene.
        TryRegisterNowOrLater();
    }

    private void OnDisable()
    {
        if (_registerRoutine != null)
        {
            StopCoroutine(_registerRoutine);
            _registerRoutine = null;
        }

        if (_isRegistered)
        {
            DirectorManager.Instance?.UnregisterActor(this);
            _isRegistered = false;
        }
    }

    private void TryRegisterNowOrLater()
    {
        if (_isRegistered) return;

        if (DirectorManager.Instance != null)
        {
            DirectorManager.Instance.RegisterActor(this);
            _isRegistered = true;
            return;
        }

        if (_registerRoutine == null)
            _registerRoutine = StartCoroutine(RegisterWhenDirectorReady());
    }

    private IEnumerator RegisterWhenDirectorReady()
    {
        // Wait until the singleton exists (handles script execution order).
        while (DirectorManager.Instance == null)
            yield return null;

        DirectorManager.Instance.RegisterActor(this);
        _isRegistered = true;
        _registerRoutine = null;
    }
}