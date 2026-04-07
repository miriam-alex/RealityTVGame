using UnityEngine;
using TMPro;
using System.Collections;

public class TextFade : MonoBehaviour
{
    [Tooltip("The TextMeshProUGUI component to fade.")]
    public TextMeshProUGUI textToFade;

    [Tooltip("Duration of the fade effect in seconds.")]
    public float fadeDuration = 1.0f;

    [Tooltip("How long the text remains fully visible.")]
    public float holdDuration = 1.0f;

    [Tooltip("How long the text remains fully invisible before looping.")]
    public float pauseDuration = 0.5f;

    void OnEnable()
    {
        if (textToFade == null)
        {
            textToFade = GetComponent<TextMeshProUGUI>();
        }

        if (textToFade != null)
        {
            StartCoroutine(LoopingFade());
        }
        else
        {
            Debug.LogError("TextFade: No TextMeshProUGUI component found or assigned.");
        }
    }

    private IEnumerator LoopingFade()
    {
        while (true)
        {
            // --- Fade In ---
            float timer = 0f;
            while (timer < fadeDuration)
            {
                float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
                textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, 1);

            // --- Hold ---
            yield return new WaitForSeconds(holdDuration);

            // --- Fade Out ---
            timer = 0f;
            while (timer < fadeDuration)
            {
                float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
                textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            textToFade.color = new Color(textToFade.color.r, textToFade.color.g, textToFade.color.b, 0);

            // --- Pause ---
            yield return new WaitForSeconds(pauseDuration);
        }
    }
}
