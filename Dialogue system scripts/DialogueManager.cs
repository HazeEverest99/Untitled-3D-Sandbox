using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [SerializeField] private DialogueChoiceSlot dialogueChoiceSlotPrefab;
    [SerializeField] private Transform dialoguesChoicesContainer;
    [SerializeField] private GameObject dialogueBoxUI;
    [SerializeField] private TextMeshProUGUI dialogueText;
    private InventoryManager inventoryManager;


    [Header("Configuration")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float typeLetterSpeed = 0.05f;

    [Header("Flags")]
    public QuestInteraction currentQuestInteraction;

    public List<DialogueChoiceSlot> dialogueChoiceSlotsIsntanced = new List<DialogueChoiceSlot>();
    public DialogueSO CurrentDialogue { get; private set; }
    public FirstPersonLook playerLook;

    private int currentDialogueIndex = 0;
    private bool hasAnswered = false;
    private bool writingDialogue;
    private bool dialogueActive;
    public VendorInstance currentVendor;

    public bool DialogueActive { get { return dialogueActive; } }
    private void Awake()
    {
        instance = this;

        dialogueBoxUI.SetActive(false);

        playerLook = FindObjectOfType<FirstPersonLook>();

        inventoryManager = InventoryManager.instance;
    }

    private void Update()
    {
        // Used to skip dialogue
        if (writingDialogue)
        {
            if (Input.GetKeyDown(interactKey))
            {
                writingDialogue = false; 
            }
        }
        if (dialogueActive)
        {
            inventoryManager.canUseItem = false;
            inventoryManager.canUseInventory = false;
        }
        else
        {
            inventoryManager.canUseItem = true;
            inventoryManager.canUseInventory = true;
        }
    }

    public void StartDialogue(DialogueSO dialogueSO,QuestInteraction questInteraction)
    {
        if (dialogueActive) return;
        currentQuestInteraction = questInteraction;
        currentDialogueIndex = 0;
        StopAllCoroutines();
        CurrentDialogue = dialogueSO;
        DisplayDialogue();
        playerLook.ToggleCursor(true);
        InventoryManager.instance.canUseItem = false;
    }

    private void DisplayDialogue()
    {
        StartCoroutine(DisplayDialogueSequence());
    }

    private IEnumerator DisplayDialogueSequence()
    {
        
        dialogueActive = true;
        dialogueBoxUI.SetActive(true);
        dialogueText.gameObject.SetActive(false);

        for (int i = 0; i < dialogueChoiceSlotsIsntanced.Count; i++)
        {
            Destroy(dialogueChoiceSlotsIsntanced[i].gameObject);
        }
        dialogueChoiceSlotsIsntanced.Clear();
        yield return new WaitForSeconds(0.1f);

        if (currentDialogueIndex < CurrentDialogue.dialogueEntries.Length)
        {
            DialogueSO.DialogueEntry currentEntry = CurrentDialogue.dialogueEntries[currentDialogueIndex];
            dialogueText.text = string.Empty;
            dialogueText.gameObject.SetActive(true);

            writingDialogue = true;
            foreach (char letter in currentEntry.npcLine)
            {
                if (!writingDialogue) break;
                dialogueText.text += letter;
                yield return new WaitForSeconds(typeLetterSpeed);
            }
            writingDialogue = false;
            dialogueText.text = currentEntry.npcLine;
            yield return new WaitForSeconds(0.1f);

            PlayerResponse[] playerResponses = currentEntry.PlayerResponses;
            yield return new WaitUntil(() => Input.GetKeyDown(interactKey));

            if (playerResponses.Length > 0)
            {
                hasAnswered = false;
               // dialogueText.gameObject.SetActive(false);

                for (int j = 0; j < playerResponses.Length; j++)
                {
                    if (playerResponses[j].isChoiceForQuest)
                    {
                        if (currentQuestInteraction.FindAvailableQuest() == null) continue;
                        playerResponses[j].DialogueToTrigger = currentQuestInteraction.FindAvailableQuest().QuestDialogue;
                        if (playerResponses[j].DialogueToTrigger == null) continue;
                    }
                    DialogueChoiceSlot dialogueChoiceSlotInstance = Instantiate(dialogueChoiceSlotPrefab, dialoguesChoicesContainer);
                    dialogueChoiceSlotInstance.SetData(playerResponses[j]);
                    dialogueChoiceSlotsIsntanced.Add(dialogueChoiceSlotInstance);
                }
                yield return new WaitUntil(() => hasAnswered);
            }


            int additional = 0;
            if(CurrentDialogue.dialogueEntries.Length == 1)
            {
                additional = 1;
            }
            currentDialogueIndex++;
            if(currentDialogueIndex == CurrentDialogue.dialogueEntries.Length)
            {
                // End dialogue
                dialogueActive = false;
                dialogueBoxUI.SetActive(false);
                playerLook.ToggleCursor(false);
                InventoryManager.instance.canUseItem = true;

            }
            else
            {
                if(playerResponses.Length == 0)
                    DisplayDialogue();
            }
        }
    }

    public void OnSubmitAnswer(PlayerResponse playerResponse)
    {
        switch(playerResponse.action)
        {
            case DialogueActionType.OpenShopBuy:
                if(currentVendor != null)
                {
                    ShopUI.instance.Open(currentVendor, ShopMode.Buy);
                }
                return;
            case DialogueActionType.OpenShopSell:
                if(currentVendor != null)
                {
                    ShopUI.instance.Open(currentVendor, ShopMode.Sell);
                }
                return;
            case DialogueActionType.LeaveDialogue:
                dialogueActive = false;
                dialogueBoxUI.SetActive(false);
                playerLook.ToggleCursor(false);
                inventoryManager.canUseItem = true;
                return;
        }
        if(playerResponse.DialogueToTrigger == null)
        {
            hasAnswered = true;
            DisplayDialogue();
        }
        else
        {
            dialogueActive = false;
            
            StartDialogue(playerResponse.DialogueToTrigger,currentQuestInteraction);
        }

        if(playerResponse.QuestToTrigger != null)
        {
            if (!currentQuestInteraction.AcceptQuest(playerResponse.QuestToTrigger))
            {
                if(playerResponse.DialogueToTriggerIfCantCompleteQuest != null)
                {            
                    dialogueActive = false;
                    StartDialogue(playerResponse.DialogueToTriggerIfCantCompleteQuest, currentQuestInteraction);
                }
            }
            else
            {
                if (playerResponse.DialogueToTriggerIfCompletedQuest != null)
                {
                    dialogueActive = false;
                    StartDialogue(playerResponse.DialogueToTriggerIfCompletedQuest, currentQuestInteraction);
                }
            }
        }
    }
}
