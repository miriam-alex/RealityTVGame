using UnityEngine;
using TMPro;
using System.Collections;

public class ViewerCounter : MonoBehaviour
{
    public TMP_Text viewerText;

    public int viewerCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        viewerCount = Random.Range(800, 1500);
        StartCoroutine(FluctuateViewers());
    }

    IEnumerator FluctuateViewers()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(1f, 3f));
            viewerCount += Random.Range(-20, 50);
            viewerCount = Mathf.Max(0, viewerCount);
            viewerText.text = viewerCount.ToString("N0") + " viewers";
        }
    }
}
