using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerInventory : MonoBehaviour
{
    [Header("Settings")]
    public Transform carryPoint;
    public float stackOffset = 0.4f; 
    public int maxCapacity = 5;

    private List<Grabbable> _heldItems = new List<Grabbable>();
    private PlayerIdentity _myId;

    public bool HasItems => _heldItems.Count > 0;
    public bool IsFull => _heldItems.Count >= maxCapacity;
    public int ItemCount => _heldItems.Count;

    public List<Grabbable> GetItems() => _heldItems;
        
    void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
    }
    
    public int CountItemsOfType(string itemType)
    {
        int count = 0;
        foreach (var item in _heldItems)
        {
            if (item.name.Contains(itemType))
                count++;
        }
        return count;
    }
    
    public bool RemoveItemsOfType(string itemType, int amount)
    {
        int removed = 0;
        for (int i = _heldItems.Count - 1; i >= 0 && removed < amount; i--)
        {
            if (_heldItems[i].name.Contains(itemType))
            {
                _heldItems[i].OnDropped();
                Destroy(_heldItems[i].gameObject);
                _heldItems.RemoveAt(i);
                removed++;
            }
        }
        return removed == amount;
    }

    public void AddItem(Grabbable item)
    {
        if (IsFull) return;

        _heldItems.Add(item);
        
        item.OnPickedUp();
        item.transform.SetParent(carryPoint);
        float verticalOffset = (_heldItems.Count - 1) * stackOffset;
        item.transform.localPosition = new Vector3(0, verticalOffset, 0);
    }

    public Grabbable DropLastItem()
    {
        if (!HasItems)
        {
            Debug.Log("Cannot DropLastItem: No items in inventory!");
            return null;
        }

        int lastIndex = _heldItems.Count - 1;
        Grabbable itemToDrop = _heldItems[lastIndex];

        itemToDrop.transform.SetParent(null);
        itemToDrop.OnDropped();

        _heldItems.RemoveAt(lastIndex);
        return itemToDrop;
    }
    
    public void TryDrop()
    {
        if (HasItems) 
        {
            DropLastItem();
        }
    }

    public bool TransferToPlayerInventory(PlayerInventory recieverInventory)
    {
        Assert.IsTrue(recieverInventory != null);
        if (!HasItems) return false;
        Grabbable item = this.DropLastItem();
        recieverInventory.AddItem(item);
        return true;
    }
}