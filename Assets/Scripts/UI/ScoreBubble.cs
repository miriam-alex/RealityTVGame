using UnityEngine;
using TMPro;

public class ScoreBubble : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float duration = 1.2f;
    private TMP_Text _text;
    private float _elapsed;

    void Awake() => _text = GetComponent<TMP_Text>();

    void Update()
    {
        // Move Up
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;
        
        // Face the Camera (so it's always readable)
        if (Camera.main != null)
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);

        // Fade Out
        _elapsed += Time.deltaTime;
        float alpha = Mathf.Lerp(1, 0, _elapsed / duration);
        _text.color = new Color(_text.color.r, _text.color.g, _text.color.b, alpha);

        if (_elapsed >= duration) Destroy(gameObject);
    }

    public void Setup(int score)
    {
        _text.text = score >= 0 ? $"+{score}" : $"{score}";
        // Optional: Change color to red if negative
        if (score < 0) _text.color = Color.red;
    }
}