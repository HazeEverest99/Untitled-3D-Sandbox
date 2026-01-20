using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ItemObject : MonoBehaviour, IInteractable  
{
    public ItemData item;


    // returns the prompt we want to show on-screen when hovering over the item
    public string GetInteractPrompt()
    {
        return string.Format("Pickup {0}", item.displayName);
    }
    // called when we interact with the item
    public void OnInteract()
    {
        // Here's an example of wha you can now do with the inventory system
        // First, declare a quantity, if the item isn't stackable then set it to 1
        int quantity = 1;
        bool tryAddItem = InventoryManager.instance.AddItem(item, quantity);

        // If the item wasn't added for any reason(inv full e.t.c) then don't pick up the item
        if (!tryAddItem) return;

        Debug.Log("Picked up " + item.displayName);
        Destroy(gameObject);
    }
}