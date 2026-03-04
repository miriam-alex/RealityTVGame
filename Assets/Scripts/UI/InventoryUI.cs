using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public PlayerInventory playerInventory; 
    public GameObject slotPrefab;
    public Transform slotParent;

    private List<GameObject> slots = new List<GameObject>();

    private void Start()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshInventory;
        }

        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshInventory;
        }
    }

    public void RefreshInventory()
    {
        if (playerInventory == null)
        {
            Debug.LogWarning("InventoryUI has no PlayerInventory assigned!");
            return;
        }

        // Clear old slots
        foreach (var slot in slots)
        {
            Destroy(slot);
        }
        slots.Clear();

        Debug.Log("Cleared Slots");

        // Create slots
        foreach (var resource in playerInventory.resources)
        {
            GameObject slot = Instantiate(slotPrefab, slotParent);

            TextMeshProUGUI text = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = resource.resourceName;
            }

            Image[] images = slot.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.gameObject.name == "IconImage") // <-- name of your child image
                {
                    img.sprite = resource.resourceIcon;
                    img.enabled = resource.resourceIcon != null;
                    img.color = resource.tint;
                    break;
                }
            }


            slots.Add(slot);
        }
    }
}