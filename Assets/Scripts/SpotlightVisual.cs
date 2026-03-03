using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SpotlightVisual : MonoBehaviour
{
    private MeshRenderer renderer;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.color = Color.yellow;
    }

    public void FlashRed(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(Flash(duration));
    }

    IEnumerator Flash(float duration)
    {
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(duration);
        renderer.material.color = Color.yellow;
    }
}