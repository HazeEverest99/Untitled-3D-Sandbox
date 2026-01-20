using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShopRowUI : MonoBehaviour
{
    public Button button;
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI priceText;
    public TextMeshProUGUI stockText;

    public VendorEntry Entry { get; private set; }

    public void Bind(VendorEntry entry, string price, string stock, Action onClick)
    {
        Entry = entry;
        icon.sprite = entry.item.icon;
        nameText.text = entry.item.displayName;
        priceText.text = price;
        stockText.text = stock;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }
}
