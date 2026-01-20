using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "New Vendor Definition")]
public class VendorDefinition : ScriptableObject
{
    [Header("Identity")]
    public string vendorName;
    
    public string displayName;

    [Header("Pricing")]
    [Tooltip("Multiplier applied to item basePrice")]
    public float buyMultiplier = 1f;

    [Range(0f, 1f)]
    [Tooltip("Sell price = buy price * sellBackRatio")]
    public float sellBackRatio = 0.33f;

    [Header("Catalog")]
    public List<VendorEntry> sellsToPlayer = new();
    public List<VendorEntry> buysFromPlayer = new();


}

[Serializable]
public class VendorEntry
{
    public ItemData item;

    [Header("Stock")]
    public bool infiniteStock = true;
    
    [Tooltip("Only used if infiniteStock is false")]
    public int startingStock = 0;

    [Header("Optional")]
    [Tooltip("If 0, uses item's basePrice")]
    public int overrideBasePrice = 0;
}
