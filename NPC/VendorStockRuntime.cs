using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class VendorStockRuntime
{
    [Serializable]
    public class ItemQty
    {
        public string itemID;
        public int qty;

    }

    public List<ItemQty> finiteStock = new();

    public int GetQty(string itemID)
    {
        var line = finiteStock.Find(x => x.itemID == itemID);
        return line == null ? 0 : line.qty;
    }

    public void SetQty(string itemID, int qty)
    {
        var line = finiteStock.Find(x => x.itemID == itemID);
        if (line == null)
        {
            finiteStock.Add(new ItemQty() { itemID = itemID, qty = qty });
        }
        else
        {
            line.qty = qty;
        }
    }
}
