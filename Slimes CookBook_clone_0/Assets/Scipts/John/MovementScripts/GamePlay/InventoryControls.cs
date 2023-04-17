using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class InventoryControls : NetworkBehaviour
{
    [System.Serializable]
    public struct Item
    {
        public Items ObjectsToPut;
        public int amount;       
    }
    public List<Item> _Object;

    public readonly SyncDictionary<string, Item> InventoryBag = new SyncDictionary<string, Item>();

    public override void OnStartServer()
    {
        enabled = true;
        foreach (Item itemToPlace in _Object)
        {            
            InventoryBag.Add(itemToPlace.ObjectsToPut.ItemName, itemToPlace);
        }    
    }
    public override void OnStartClient()
    {
        // Equipment is already populated with anything the server set up
        // but we can subscribe to the callback in case it is updated later on
        InventoryBag.Callback += OnInventoryChange;

        // Process initial SyncDictionary payload
        foreach (KeyValuePair<string, Item> kvp in InventoryBag)
            OnInventoryChange(SyncDictionary<string, Item>.Operation.OP_ADD, kvp.Key, kvp.Value);
    }

    void OnInventoryChange(SyncIDictionary<string, Item>.Operation op, string key, Item item)
    {
        switch (op)
        {
            case SyncIDictionary<string, Item>.Operation.OP_ADD:
                // entry added
               
                break;
            case SyncIDictionary<string, Item>.Operation.OP_SET:
                // entry changed              
                break;
            case SyncIDictionary<string, Item>.Operation.OP_REMOVE:
                // entry removed
               
                break;
            case SyncIDictionary<string, Item>.Operation.OP_CLEAR:
                // Dictionary was cleared

                break;
        }
    }
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            InventoryBag.TryGetValue("Water", out Item objects);
            objects.amount += 1;
            InventoryBag["Water"] = objects;
            
        }
    }
}
