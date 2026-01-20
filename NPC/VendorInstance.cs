using UnityEngine;

public class VendorInstance : MonoBehaviour
{
    [Header("Definition + Dialogue")]
    public VendorDefinition definition;
    public DialogueSO greetingDialogue;

    [Header("Per-instance runtime stock")]
    public VendorStockRuntime runtimeStock = new VendorStockRuntime();

    void Awake()
    {
        InitializeStockIfNeeded();
    }

    void InitializeStockIfNeeded()
    {
        if (definition == null) return;

        if (runtimeStock.finiteStock.Count > 0) return;

        foreach (var entry in definition.sellsToPlayer)
        {
            if (entry.item == null) continue;
            if (entry.infiniteStock) continue;

            runtimeStock.SetQty(entry.item.itemId, entry.startingStock);

        }
    }

}
