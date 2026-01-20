using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance = null;

    public Action<HotbarSlot> onHotbarSelected;

    [Header("General")]
    [SerializeField] private Transform hotbarContainer = null;

    [Header("UI")]
    [SerializeField] private GameObject descriptionUI = null;
    [SerializeField] private TextMeshProUGUI selectedItemNameUILabel;
    [SerializeField] private TextMeshProUGUI selectedItemDescriptionUILabel;
    [SerializeField] private TextMeshProUGUI selectedItemStatNamesUILabel;
    [SerializeField] private TextMeshProUGUI selectedItemStatValuesUILabel;

    public UIFader descriptionUIFader;

    public HotbarSlot SelectedHotbar { get; private set ;}
    public List<ItemInstance> ItemInstances { get; private set; } = new List<ItemInstance>();
    public ItemInstance CurrentItemInstance;

    private HotbarSlot[] hotbarSlots;
    private PlayerNeeds playerNeeds;
    public Transform dropPosition;
    public int currentHotbarIndex = 0;
    private float scrollTimer = 0.0f;
    private const float scrollThreshold = 0.1f;
    private int curEquipIndex;
    public bool inventoryTooFull = false;
    public bool canUseItem = true;
    public bool canUseInventory = true;

    private void Awake()
    {
        instance =this;
        descriptionUIFader = descriptionUI.GetComponent<UIFader>();
    }

    private void SetUp()
    {
        hotbarSlots = hotbarContainer.GetComponentsInChildren<HotbarSlot>();
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            ItemInstances.Add(new ItemInstance(null,0));
            hotbarSlots[i].SetUpData(ItemInstances[i]);
        }
        SelectSlot(0);
    }

    void Start()
    {
        onHotbarSelected += OnHotbarSelected;
        SetUp();
        playerNeeds = FindObjectOfType<PlayerNeeds>();     
    }

    private void Update()
    {
        if (canUseInventory == false) return;
        ProcessHotbarInput();

        if(Input.GetMouseButtonDown(0) && SelectedHotbar.ItemInstance.ItemData != null && SelectedHotbar.ItemInstance.ItemData.type == ItemType.Consumable)
        {
            UseItem();
        }

        if(Input.GetKeyDown(KeyCode.Q) && SelectedHotbar.ItemInstance.ItemData != null)
        {
            ThrowItem(SelectedHotbar.ItemInstance.ItemData);
        }
    }

    private void ProcessHotbarInput()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
                break;
            }
        }
        ProcessHotbarScrolling();
    }

    private void ProcessHotbarScrolling()
    {
        scrollTimer += Time.deltaTime;
      
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel > 0)
        {
            if (scrollTimer < scrollThreshold) return;
            scrollTimer = 0.0f;
            if (currentHotbarIndex + 1 < hotbarSlots.Length)
            {
                currentHotbarIndex++;
            }
            else
            {
                currentHotbarIndex = 0;
            }
            SelectSlot(currentHotbarIndex);
        }
        else if (scrollWheel < 0)
        {
            if (scrollTimer < scrollThreshold) return;
            scrollTimer = 0.0f;
            if (currentHotbarIndex - 1 >= 0)
            {
                currentHotbarIndex--;
            }
            else
            {
                currentHotbarIndex = hotbarSlots.Length - 1;
            }
            SelectSlot(currentHotbarIndex);
        }
    }

    private void OnHotbarSelected(HotbarSlot hotbarSlot)
    {
        if(hotbarSlot.ItemInstance.ItemData == null && descriptionUI.activeSelf == true)
        {
            descriptionUIFader.FadeOut(0.25f);
        }
        else
        {
            descriptionUIFader.FadeIn(0.25f);
            selectedItemNameUILabel.text = hotbarSlot.ItemInstance.ItemData.displayName;
            selectedItemDescriptionUILabel.text = hotbarSlot.ItemInstance.ItemData.description;
            selectedItemStatNamesUILabel.text = "";
            selectedItemStatValuesUILabel.text = "";

            for (int i = 0; i < hotbarSlot.ItemInstance.ItemData.consumables.Length; i++)
            {
                selectedItemStatNamesUILabel.text += hotbarSlot.ItemInstance.ItemData.consumables[i].type + "\n";
                selectedItemStatValuesUILabel.text += hotbarSlot.ItemInstance.ItemData.consumables[i].value + "\n";
            }
            
           
           // descriptionUIFader.FadeOut(1f);
        }
    }
    public void SelectSlot(int index)
    {
        SelectedHotbar?.Deselect();
        currentHotbarIndex = index;
        SelectedHotbar = hotbarSlots[currentHotbarIndex];
        SelectedHotbar.Select();
        onHotbarSelected?.Invoke(SelectedHotbar);

        Equip(SelectedHotbar.ItemInstance);
    }

    public bool AddItem(ItemData itemData, int quantity)
    {
        ItemInstance itemInstance = FindAvailableItemToStack(itemData);
        if(itemInstance == null)
        {
            // Add new instance
            ItemInstance emptyItemInstance = FindAvailableItemInstance();
            if(emptyItemInstance == null)
            {
                if(InteractionManager.instance.curInteractGameObject != null)
                {
                    Destroy(InteractionManager.instance.curInteractGameObject);
                }
                Debug.Log("Inventory is full!");
                inventoryTooFull = true;
                ThrowItem(itemData);
                return false;
            }
            emptyItemInstance.ItemData = itemData;
            emptyItemInstance.Quantity =quantity;

            if(itemData.canStack == false)
            {
                emptyItemInstance.Quantity = 1;
            }
        }
        else
        {
            // Stack
            itemInstance.Quantity += quantity;
            if(itemInstance.Quantity > itemData.maxStackAmount)
            {
                // Remove difference if stack exceeded
                int difference = itemInstance.Quantity - itemData.maxStackAmount;
                itemInstance.Quantity -= difference;

                // Attempt to stack the difference
                AddItem(itemData, difference);
            }
        }
        onHotbarSelected?.Invoke(SelectedHotbar);
        return true;
    }

    public void ThrowItem(ItemData itemData)
    {
        if (!inventoryTooFull)
        { 
            RemoveItem(itemData, 1);
        }

        Instantiate(itemData.dropPrefab, dropPosition.position, Quaternion.Euler(360.0f * UnityEngine.Random.value * Vector3.one));
        inventoryTooFull = false;
    }

    public void RemoveItem(ItemData itemData, int quantity)
    {
        ItemInstance itemInstance = FindAvailableItemInstanceToRemove(itemData);
        if (itemInstance != null)
        {
            itemInstance.Quantity -= quantity;
            if (itemInstance.Quantity <= 0.0f)
            {
                itemInstance.Clear();
            }
        }
        else
        {
            Debug.LogError("You're trying to remove an item that doesn't exist");
        }
        onHotbarSelected?.Invoke(SelectedHotbar);

        if(SelectedHotbar.ItemInstance.ItemData == null)
        {
            UnEquip();
        }
    }

    public void CalculateRemainingHotbarSlots()
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
        {
            if(hotbarSlots[i].ItemInstance.ItemData == null)
            {
                hotbarSlots[i].ItemInstance.Quantity = 0;
            }
        }
    }

    // Helpers
    private ItemInstance FindAvailableItemToStack(ItemData itemData)
    {
        for (int i = 0; i < ItemInstances.Count; i++)
        {
            if (ItemInstances[i].ItemData == null) continue;
            if (ItemInstances[i].ItemData.canStack == false) continue;

            if (ItemInstances[i].ItemData == itemData)
            {
                if (ItemInstances[i].IsMaxStacked()) continue;
                return ItemInstances[i];
            }
        }
        return null;
    }

    private ItemInstance FindAvailableItemInstanceToRemove(ItemData itemData)
    {
        for (int i = ItemInstances.Count - 1; i >= 0; i--)
        {
            if (ItemInstances[i].ItemData == itemData)
            {
                return ItemInstances[i];
            }
        }
        return null;
    }

    private ItemInstance FindAvailableItemInstance()
    {
        for (int i = 0; i < ItemInstances.Count; i++)
        {
            if (ItemInstances[i].ItemData == null) return ItemInstances[i];
        }
        return null;
    }

    private bool HasItem(ItemData itemData)
    {
        for (int i = 0; i < ItemInstances.Count; i++)
        {
            if (ItemInstances[i].ItemData == itemData)
            {
                return true;
            }
        }
        return false;
    }

    private int GetItemTotalCount(ItemData itemData)
    {
        if(!HasItem(itemData)) return 0;

        int count = 0;
        for (int i = 0; i < ItemInstances.Count; i++)
        {
            if (ItemInstances[i].ItemData == itemData)
            {
                count += ItemInstances[i].Quantity;
            }
        }
        return count;
    }

    public bool HasItems(QuestRequirement[] questRequirements)
    {
        bool hasRequirements = true;
        for (int i = 0; i < questRequirements.Length; i++)
        {
            for (int j = 0; j < ItemInstances.Count; j++)
            {
                if(HasItem(questRequirements[i].ItemData) == false)
                {
                    hasRequirements = false;
                    break;
                }
                if (ItemInstances[j].ItemData != questRequirements[i].ItemData) continue;
                if (GetItemTotalCount(ItemInstances[j].ItemData) >= questRequirements[i].ItemQuantityNeeded)
                {
                    break;
                    // Has items
                }
                else
                {
                    hasRequirements = false;
                }
            }
        }
        return hasRequirements;
    }

    void Equip (ItemInstance itemInstance)
    {
        UnEquip();
        if(itemInstance.ItemData != null && itemInstance.ItemData.type == ItemType.Equipable)
        {
            EquipManager.instance.EquipNew(itemInstance.ItemData);
            SelectedHotbar.UpdateUI();
        }
    }

    void UnEquip ()
    {
        EquipManager.instance.UnEquip();
        SelectedHotbar.UpdateUI();
    }

    public void UseItem()
    {
        if(SelectedHotbar.ItemInstance != null && SelectedHotbar.ItemInstance.ItemData.type == ItemType.Consumable && canUseItem)
        {
            for (int i = 0; i < SelectedHotbar.ItemInstance.ItemData.consumables.Length; i++)
            {
                switch (SelectedHotbar.ItemInstance.ItemData.consumables[i].type)
                {
                    case ConsumableType.Hunger: playerNeeds.Eat(SelectedHotbar.ItemInstance.ItemData.consumables[i].value); break;
                    case ConsumableType.Thirst: playerNeeds.Drink(SelectedHotbar.ItemInstance.ItemData.consumables[i].value); break;
                    case ConsumableType.Health: playerNeeds.Heal(SelectedHotbar.ItemInstance.ItemData.consumables[i].value); break;
                    case ConsumableType.Sleep: playerNeeds.Sleep(SelectedHotbar.ItemInstance.ItemData.consumables[i].value); break;
                }
            }
            RemoveItem(SelectedHotbar.ItemInstance.ItemData, 1);
        }
    }

    public bool CanAdd(ItemData item, int qty)
{
    if (item == null || qty <= 0) return false;

    // If stackable, if any existing stack has room, it's addable.
    if (item.canStack)
    {
        for (int i = 0; i < ItemInstances.Count; i++)
        {
            var inst = ItemInstances[i];
            if (inst.ItemData == item && !inst.IsMaxStacked())
                return true;
        }
    }

    // Otherwise need an empty slot
    for (int i = 0; i < ItemInstances.Count; i++)
        if (ItemInstances[i].ItemData == null)
            return true;

    return false;
}

    public bool CanRemove(ItemData item, int qty)
    {
        return GetCount(item) >= qty;
    }

    public int GetCount(ItemData item)
    {
        int total = 0;
        for (int i = 0; i < ItemInstances.Count; i++)
            if (ItemInstances[i].ItemData == item)
                total += ItemInstances[i].Quantity;
        return total;
    }

    public IEnumerable<ItemInstance> GetAllStacks()
    {
        for (int i = 0; i < ItemInstances.Count; i++)
            if (ItemInstances[i].ItemData != null)
                yield return ItemInstances[i];
    }


    
}

[System.Serializable]
public class ItemInstance
{
    public ItemData ItemData;
    public ItemDataConsumable ItemDataConsumable;
    public int Quantity = 0;

    public ItemInstance (ItemData itemData, int quantity)
    {
        ItemData = itemData;
        Quantity = quantity;
    }

    public void Clear()
    {
        ItemData = null;
        Quantity = 0;
    }

    public bool IsMaxStacked()
    {
        return ItemData.canStack && Quantity >= ItemData.maxStackAmount;
    }
    
        
}