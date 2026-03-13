using UnityEngine;
using UnityEngine.UI;

public class FadeInOut : MonoBehaviour
{
    public float fadeSpeed = 1f;
    private Image image;
    private bool fadingIn = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        image = GetComponent<Image>();
        Color c = image.color;
        c.a = 0f;
        image.color = c;
    }

    // Update is called once per frame
    void Update()
    {
        Color c = image.color;
        if (fadingIn)
        {
            c.a += fadeSpeed * Time.deltaTime;
            if(c.a >= 1f) fadingIn = false;
        }
        else
        {
            {
                c.a -= fadeSpeed * Time.deltaTime;
                if (c.a <= 0f) fadingIn = true;
            }
        }
        
        image.color = c;
    }
}
