using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public enum ShopMode { Buy, Sell }

public class ShopUI : MonoBehaviour
{
    public static ShopUI instance;

    [Header("Root")]
    public GameObject root;

    [Header("Top")]
    public TextMeshProUGUI vendorNameText;
    public TextMeshProUGUI moneyText;

    [Header("List")]
    public Transform contentRoot;
    public ShopRowUI rowPrefab;

    [Header("Details")]
    public Image detailsIcon;
    public TextMeshProUGUI detailsName;
    public TextMeshProUGUI detailsDesc;
    public TextMeshProUGUI detailsPrice;
    public TextMeshProUGUI detailsStock;

    [Header("Quantity")]
    public TMP_InputField qtyInput;
    public Button minusButton;
    public Button plusButton;

    [Header("Confirm/Close")]
    public Button confirmButton;
    public TextMeshProUGUI confirmText;
    public Button closeButton;

    VendorInstance _vendor;
    ShopMode _mode;

    VendorEntry _selectedEntry;
    ItemData _selectedItem;

    readonly List<ShopRowUI> _rows = new();
    [Header("UI to hide when shop opens")]
    [SerializeField] private List<GameObject> uiToDisable = new();
    private Dictionary<GameObject, bool> _previousStates = new();

    [Header("Modal + Fade")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private bool closeWithEscape = true;

    
    [Header("Disable while shop open (player control scripts)")]
    [Tooltip("Drag PlayerMovement, InteractionManager, CameraLook, etc. Anything you want disabled while shopping.")]
    [SerializeField] private List<MonoBehaviour> disableWhileOpen = new();

    private Coroutine fadeRoutine;
    private bool isOpen;



    void Awake()
    {
        instance = this;
        root.SetActive(false);

        minusButton.onClick.AddListener(() => AdjustQty(-Step()));
        plusButton.onClick.AddListener(() => AdjustQty(+Step()));
        qtyInput.onValueChanged.AddListener(_ => RefreshSelection());

        confirmButton.onClick.AddListener(Confirm);
        closeButton.onClick.AddListener(Close);

    }

    private void Update()
    {
        if (!isOpen) return;

        if (closeWithEscape && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    int Step() => (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? 10 : 1;

    public void Open(VendorInstance vendor, ShopMode mode)
    {
        if (isOpen) return;
        _vendor = vendor;
        _mode = mode;

        // Modal enable
        ApplyModalState(true);

        // Show root and fade in
        root.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            BeginFade(0f, 1f);
        }
        
        vendorNameText.text = vendor.definition.displayName;

        // Keep cursor ON because dialogue already turned it on for choices
        RebuildList();
        SelectFirst();
        RefreshMoney();
    }

    public void Close()
    {
        if (!isOpen) return;

        // Fade out, then actually disable
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            BeginFade(canvasGroup.alpha, 0f, () =>
            {
                root.SetActive(false);
                CleanupAfterClose();
            });
        }
        else
        {
            root.SetActive(false);
            CleanupAfterClose();
        }
    }

    private void CleanupAfterClose()
    {
        ClearRows();
        _vendor = null;
        _selectedEntry = null;
        _selectedItem = null;

        ApplyModalState(false);
    }

    void RefreshMoney()
    {
        moneyText.text = $"${MoneyManager.instance.money}";
    }

    void ClearRows()
    {
        foreach (var r in _rows) Destroy(r.gameObject);
        _rows.Clear();
    }

    void RebuildList()
    {
        ClearRows();

        var def = _vendor.definition;

        if (_mode == ShopMode.Buy)
        {
            foreach (var entry in def.sellsToPlayer)
            {
                if (entry.item == null) continue;

                int each = ShopService.GetBuyPrice(entry.item, def);
                string stock = entry.infiniteStock ? "∞" : _vendor.runtimeStock.GetQty(entry.item.itemId).ToString();

                var row = Instantiate(rowPrefab, contentRoot);
                _rows.Add(row);
                row.Bind(entry, $"${each}", stock, () => Select(entry));
            }
        }
        else
        {
            foreach (var stack in InventoryManager.instance.GetAllStacks())
            {
                var entry = def.buysFromPlayer.Find(e => e.item == stack.ItemData);
                if (entry == null) continue;

                int each = ShopService.GetSellPrice(entry.item, def);
                string have = InventoryManager.instance.GetCount(entry.item).ToString();

                var row = Instantiate(rowPrefab, contentRoot);
                _rows.Add(row);
                row.Bind(entry, $"+${each}", have, () => Select(entry));
            }
        }
    }

    void Select(VendorEntry entry)
    {
        _selectedEntry = entry;
        _selectedItem = entry.item;
        qtyInput.text = "1";
        RefreshSelection();
    }

    void SelectFirst()
    {
        if (_rows.Count > 0)
            Select(_rows[0].Entry);
        else
            confirmButton.interactable = false;
    }

    int ReadQty()
    {
        if (int.TryParse(qtyInput.text, out int q)) return Mathf.Clamp(q, 1, 9999);
        return 1;
    }

    void AdjustQty(int delta)
    {
        int q = ReadQty();
        q = Mathf.Clamp(q + delta, 1, 9999);
        qtyInput.text = q.ToString();
    }

    void RefreshSelection()
    {
        if (_selectedItem == null || _selectedEntry == null)
        {
            confirmButton.interactable = false;
            return;
        }

        detailsIcon.sprite = _selectedItem.icon;
        detailsName.text = _selectedItem.displayName;
        detailsDesc.text = _selectedItem.description;

        int qty = ReadQty();

        if (_mode == ShopMode.Buy)
        {
            int each = ShopService.GetBuyPrice(_selectedItem, _vendor.definition);
            int total = each * qty;

            detailsPrice.text = $"${each} x {qty} = ${total}";
            detailsStock.text = _selectedEntry.infiniteStock ? "Stock: ∞" : $"Stock: {_vendor.runtimeStock.GetQty(_selectedItem.itemId)}";

            bool stockOk = _selectedEntry.infiniteStock || _vendor.runtimeStock.GetQty(_selectedItem.itemId) >= qty;
            bool invOk = InventoryManager.instance.CanAdd(_selectedItem, qty);
            bool moneyOk = MoneyManager.instance.CanAfford(total); // you already have this :contentReference[oaicite:5]{index=5}

            confirmText.text = "Buy";
            confirmButton.interactable = stockOk && invOk && moneyOk;
        }
        else
        {
            int each = ShopService.GetSellPrice(_selectedItem, _vendor.definition);
            int total = each * qty;

            detailsPrice.text = $"+${each} x {qty} = +${total}";
            detailsStock.text = $"You have: {InventoryManager.instance.GetCount(_selectedItem)}";

            bool invOk = InventoryManager.instance.CanRemove(_selectedItem, qty);

            confirmText.text = "Sell";
            confirmButton.interactable = invOk;
        }

        RefreshMoney();
    }

    void Confirm()
    {
        if (_vendor == null || _selectedItem == null || _selectedEntry == null) return;

        int qty = ReadQty();

        if (_mode == ShopMode.Buy)
        {
            int each = ShopService.GetBuyPrice(_selectedItem, _vendor.definition);
            int total = each * qty;

            if (!InventoryManager.instance.CanAdd(_selectedItem, qty)) return;
            if (!MoneyManager.instance.CanAfford(total)) return;

            MoneyManager.instance.SpendMoney(total); // exists :contentReference[oaicite:6]{index=6}

            if (!_selectedEntry.infiniteStock)
            {
                int left = _vendor.runtimeStock.GetQty(_selectedItem.itemId) - qty;
                _vendor.runtimeStock.SetQty(_selectedItem.itemId, left);
            }

            InventoryManager.instance.AddItem(_selectedItem, qty); // exists :contentReference[oaicite:7]{index=7}
        }
        else
        {
            if (!InventoryManager.instance.CanRemove(_selectedItem, qty)) return;

            int each = ShopService.GetSellPrice(_selectedItem, _vendor.definition);
            int payout = each * qty;

            InventoryManager.instance.RemoveItem(_selectedItem, qty); // exists :contentReference[oaicite:8]{index=8}
            MoneyManager.instance.AddMoney(payout); // exists :contentReference[oaicite:9]{index=9}
        }

        RebuildList();
        RefreshSelection();
    }

    void HideOtherUI()
    {
        _previousStates.Clear();

        foreach (var go in uiToDisable)
        {
            if (go == null) continue;

            _previousStates[go] = go.activeSelf;
            go.SetActive(false);
        }
    }

    void RestoreOtherUI()
    {
        foreach (var kvp in _previousStates)
        {
            if (kvp.Key != null)
                kvp.Key.SetActive(kvp.Value);
        }

        _previousStates.Clear();
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (canvasGroup == null) yield break;

        canvasGroup.alpha = from;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // important: works even if you pause time later
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void BeginFade(float from, float to, System.Action onComplete = null)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeThen(from, to, fadeDuration, onComplete));
    }

    private IEnumerator FadeThen(float from, float to, float duration, System.Action onComplete)
    {
        yield return Fade(from, to, duration);
        onComplete?.Invoke();
    }

    
    private void ApplyModalState(bool open)
    {
        isOpen = open;

        // Lock/unlock gameplay scripts
        foreach (var mb in disableWhileOpen)
        {
            if (mb == null) continue;
            mb.enabled = !open;
        }

        // Optional: hide other UI (hotbar, crosshair, interact prompt, etc.)
        if (open) HideOtherUI();
        else RestoreOtherUI();

        // Ensure inventory usage is disabled while shopping (matches your existing patterns)
        if (InventoryManager.instance != null)
        {
            InventoryManager.instance.canUseItem = !open;
            InventoryManager.instance.canUseInventory = !open;
        }

        // Cursor state (keep it simple)
        Cursor.visible = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
    }



}
