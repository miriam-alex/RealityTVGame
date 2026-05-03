using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 2, 0); // Keep arrow above the object

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position + offset;
            transform.LookAt(target.position); // Point toward target
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}