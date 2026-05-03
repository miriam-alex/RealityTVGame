using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerInventory : MonoBehaviour
{
    [Header("Settings")]
    public float stackOffset = 0.4f; 
    public int maxCapacity = 5;

    private List<Grabbable> _heldItems = new List<Grabbable>();
    private PlayerIdentity _myId;

    public bool HasItems => _heldItems.Count > 0;
    public bool IsFull => _heldItems.Count >= maxCapacity;
    public int ItemCount => _heldItems.Count;

    public List<Grabbable> GetItems() => _heldItems;
    
    [SerializeField] private Transform _carryPoint;

        
    void Start()
    {
        _myId = GetComponent<PlayerIdentity>();
        
        // carry point is located within the body of PlayerBody > whatever animal prefab > ObjectCarryPoint
        Transform bodyTransform = _myId.bodyMountPoint;
        // the body game object SHOULD ONLY have one child
        Transform animalTransform = bodyTransform.GetChild(0);
        Assert.IsNotNull(animalTransform);
        // getting ObjectCarryPoint
        _carryPoint = animalTransform.Find("ObjectCarryPoint");
        if (_carryPoint == null) {
            Debug.LogError("CARRY POINT CANNOT BE FOUND");
        }
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
        item.transform.SetParent(_carryPoint);
        float verticalOffset = (_heldItems.Count - 1) * stackOffset;
        item.transform.localPosition = new Vector3(0, verticalOffset, 0);
    }

    public bool HasItem()
    {
        // Check if the list of held items is not empty
        return _heldItems != null && _heldItems.Count > 0; 
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

    public bool TransferToPlayerInventory(PlayerInventory recieverInventory, out Grabbable transferredItem)
    {
        transferredItem = null;
        Assert.IsTrue(recieverInventory != null);
        if (!HasItems) return false;
        Grabbable item = this.DropLastItem();
        transferredItem = item;
        recieverInventory.AddItem(item);
        return true;
    }
}