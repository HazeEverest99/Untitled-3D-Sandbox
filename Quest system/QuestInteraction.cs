using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestInteraction : MonoBehaviour
{
    [field: SerializeField] public KeyCode QuestInteractionInput { get; private set; } =  KeyCode.E;

    [SerializeField] private List<QuestSO> availableQuests = new List<QuestSO>();
    [SerializeField] private GameObject availableQuestIndicator;
    public MoneyManager moneyManager;

    private QuestUI questUI = null;
    private int currentQuestIndex = 0;
    public int CurrentQuestIndex => currentQuestIndex;
    public int QuestCount => availableQuests.Count;


    private void Awake()
    {
        questUI = FindObjectOfType<QuestUI>(); //Find and assign QuestUI component
    }

    private void Update()
    {
        if(availableQuestIndicator != null) 
        {
            availableQuestIndicator.SetActive(FindAvailableQuest() != null); //Activate the available quest indicator if there is a quest available
        }
    }

    public void ShowQuestInformation()
    {
        if (FindAvailableQuest() == null) return; //If there is no quest available, return
        questUI.DisplayQuestInformation(FindAvailableQuest(), this); //Display the quest information
    }

    public QuestSO FindAvailableQuest()
    {
        if (availableQuests.Count == 0)
        {
            return null;
        }
        if (currentQuestIndex >= availableQuests.Count)
        {
            return null;
        }
       return availableQuests[currentQuestIndex]; //Return the current quest
    }

    public bool AcceptQuest(QuestSO questSO)
    {
        if (InventoryManager.instance.HasItems(questSO.QuestRequirements)) //Check if the player has the required items to accept the quest
        {
            for (int i = 0; i < questSO.reward.items.Length; i++)
            {
                InventoryManager.instance.AddItem(questSO.reward.items[i], 1); //Add the reward items to the player's inventory
            }
            for (int i = 0; i < questSO.QuestRequirements.Length; i++)
            {
                InventoryManager.instance.RemoveItem(questSO.QuestRequirements[i].ItemData, questSO.QuestRequirements[i].ItemQuantityNeeded); //Remove the required items from the player's inventory
            }
            currentQuestIndex++;
            if (questSO.reward.money > 0)
            {
                moneyManager.AddMoney(questSO.reward.money); //Add the reward money to the player's money
            }
            return true;
        }
        else
        {
            Debug.Log("You don't have the required items to accept this job.");
            return false;
        }
    }

    public bool HasCompletedQuestIndex(int index)
    {
        return currentQuestIndex > index;
    }
}
