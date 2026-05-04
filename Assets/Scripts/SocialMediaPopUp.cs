using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SocialMediaPopUp : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI followerText;
    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private AnimalDefinition animalDefinition;
    [SerializeField] private AnimalCatalog animalCatalog;
    [SerializeField] private Image iconImage;
    
    public float animationDuration = 1.5f;

    public void TriggerPopUp(int startValue, int endValue)
    {
        StartCoroutine(AnimateFollowers(startValue, endValue));
    }

    public void Initialize(AnimalDefinition def)
    {
        animalDefinition = def;
        iconImage.sprite = def.icon;
        usernameText.text = $"@{animalDefinition.name.ToLower()}";
    }

    IEnumerator AnimateFollowers(int start, int end)
    {
        float elapsed = 0;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
        
            // Calculate the current interpolated value
            int current = (int)Mathf.Lerp(start, end, elapsed / animationDuration);
        
            // {current:N0} formats the number with commas
            // The rest is the literal string you want to append
            followerText.text = $"{current}k followers"; 
        
            yield return null;
        }
    
        // Ensure it finishes on the exact final value
        followerText.text = $"{end}k followers";
    }
}