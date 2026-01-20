using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCObject : MonoBehaviour, IInteractable
{

[SerializeField] private DialogueSO dialogueToTrigger;
[SerializeField] private QuestInteraction questInteraction;

[Header("Shop Unlock")]
public VendorInstance vendor; //optional, only if this NPC is a vendor
public bool shopRequiresQuest = false;

[Tooltip("0 = after quest 1 is completed, 1 = after quest 2 is completed, etc.")]
public int questIndexToUnlockShop = 0;

public DialogueSO dialogueBeforeShop;
public DialogueSO dialogueAfterShop;

    public NPCData npcData;
    private QuestUI questUI = null;

    private void Awake()
    {
        questUI = FindObjectOfType<QuestUI>();
    }

    public string GetInteractPrompt()
    {
        return string.Format("Talk to {0}", npcData.npcName);
    }

    public void OnInteract()
    {
        if(questUI.QuestUIGO.activeInHierarchy == true) return;

        bool shopUnlocked = true;
        if(shopRequiresQuest && questInteraction != null)
            {shopUnlocked = questInteraction.HasCompletedQuestIndex(questIndexToUnlockShop);}

        DialogueManager.instance.currentVendor = shopUnlocked ? vendor : null;

        DialogueSO chosenDialogue = dialogueToTrigger;
        if(shopRequiresQuest)
        {chosenDialogue = shopUnlocked ? (dialogueAfterShop != null ? dialogueAfterShop : dialogueToTrigger)
        : (dialogueBeforeShop != null ? dialogueBeforeShop : dialogueToTrigger);}

        DialogueManager.instance.StartDialogue(chosenDialogue, questInteraction);
        Debug.Log("Talked to " + npcData.npcName);
    }
}
