using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour
{
    [SerializeField] private Image selector = null;
    [SerializeField] private Image itemIconImage = null;
    [SerializeField] private TextMeshProUGUI quantityUILabel = null;

    private ItemData itemData = null;
    private ItemInstance itemInstance = null;

    public ItemInstance ItemInstance { get { return itemInstance; } }   

    private void Start()
    {
        SetUp();
    }

    private void SetUp()
    {
        selector.gameObject.SetActive(false);
        itemIconImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (itemInstance == null) return;
        if(itemInstance.ItemData != null) 
        {
            itemIconImage.gameObject.SetActive(true);
            itemIconImage.sprite = itemInstance.ItemData.icon;

            quantityUILabel.gameObject.SetActive(itemInstance.ItemData.canStack);
            quantityUILabel.text = itemInstance.Quantity.ToString();
        }
        else
        {
            quantityUILabel.gameObject.SetActive(false);
            itemIconImage.gameObject.SetActive(false);
        }
    }

    public void SetUpData(ItemInstance itemInstance)
    {
        this.itemInstance = itemInstance;
    }

    public void Select()
    {
        selector.gameObject.SetActive(true);
    }

    public void Deselect()
    {
        selector.gameObject.SetActive(false);
    }
}