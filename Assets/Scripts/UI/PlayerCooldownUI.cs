using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerCooldownUI : MonoBehaviour
{
    [SerializeField] private Image cooldownRadial;
    [SerializeField] private TMP_Text timerText;
    
    private PlayerInteractable _myInteractable;

    private void Awake()
    {
        _myInteractable = GetComponentInParent<PlayerInteractable>();
        SetUIActive(false); // Force hide at start
    }

    private void OnEnable()
    {
        if (_myInteractable != null)
            _myInteractable.OnCooldownStarted += StartCooldown;
    }

    private void OnDisable()
    {
        if (_myInteractable != null)
            _myInteractable.OnCooldownStarted -= StartCooldown;
    }

    public void TriggerCooldown()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(RunTimer(5.0f, 5.0f));
    }

    private void StartCooldown(float current, float total)
    {
        // This now triggers when TriggerCooldown() is called via the event
        StopAllCoroutines(); // Prevent multiple timers from overlapping
        StartCoroutine(RunTimer(current, total));
    }

    private IEnumerator RunTimer(float current, float total)
    {
        SetUIActive(true);
        float elapsed = current;
        while (elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            cooldownRadial.fillAmount = elapsed / total;
            timerText.text = elapsed.ToString("F1");
            yield return null;
        }
        SetUIActive(false);
    }

    

    private void SetUIActive(bool active)
    {
        cooldownRadial.gameObject.SetActive(active);
        timerText.gameObject.SetActive(active);
    }
}