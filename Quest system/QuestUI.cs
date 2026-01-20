using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject questUI = null; //main UI panel
    [SerializeField] private TextMeshProUGUI questTitleUILabel = null; // The title of quest
    [SerializeField] private TextMeshProUGUI questDescriptionUILabel = null; // The description of quest
    [SerializeField] private QuestInformationSlot questRequirementSlot; //Prefab for quest requirement slot
    [SerializeField] private QuestInformationSlot questRewardSlot; //Prefab for quest reward slot
    [SerializeField] private Transform questRequirementsContainer = null; //Container for quest requirements
    [SerializeField] private Transform questRewardsContainer = null; //Container for quest rewards
    [SerializeField] private Button completeQuestButton; //Button to complete quest


    private List<QuestInformationSlot> requirementSlots = new List<QuestInformationSlot>(); //List of quest requirement slots
    private List<QuestInformationSlot> rewardSlots = new List<QuestInformationSlot>();  //List of quest reward slots

    private QuestSO currentQuest; // current quest being displayed
    private QuestInteraction currentQuestInteraction; // current quest interaction

    public GameObject QuestUIGO { get { return questUI; } } // Property to access quest UI game object

    private void Start()
    {
        questUI.SetActive(false); // Deactivate quest UI panel on start

        // Clear existing child objects in quest requirements and rewards containers
        for (int i = 0; i < questRequirementsContainer.childCount; i++)
        {
            Destroy(questRequirementsContainer.GetChild(i).gameObject);
        }
        for (int i = 0; i < questRewardsContainer.childCount; i++)
        {
            Destroy(questRewardsContainer.GetChild(i).gameObject);
        }

        completeQuestButton.onClick.AddListener(OnTryCompleteQuest); // Add a listener to complete quest button
    }

    private void Update()
    {
        // Update the interactability of complete quest button based on requirements
        if (currentQuest != null)
        {
            completeQuestButton.interactable = InventoryManager.instance.HasItems(currentQuest.QuestRequirements);
        }
    }

    public void DisplayQuestInformation(QuestSO questSO, QuestInteraction questInteraction)
    {
        currentQuestInteraction = questInteraction;
        currentQuest = questSO;

        // Clear existing quest requirement and reward slots
        for (int i = 0; i < requirementSlots.Count; i++)
        {
            Destroy(requirementSlots[i].gameObject);
        }
        for (int i = 0; i < rewardSlots.Count; i++)
        {
            Destroy(rewardSlots[i].gameObject);
        }

        rewardSlots.Clear();
        requirementSlots.Clear();

        questUI.SetActive(true); // Activate quest UI panel
        questTitleUILabel.text = questSO.QuestName; // Set quest title
        questDescriptionUILabel.text = questSO.QuestDescription; // Set quest description

        // Display quest requirements
        for (int i = 0; i < questSO.QuestRequirements.Length; i++)
        {
            QuestInformationSlot questRequirementSlotInstance =
                Instantiate(questRequirementSlot, questRequirementsContainer);
            questRequirementSlotInstance.DisplayInformation("x" + questSO.QuestRequirements[i].ItemQuantityNeeded.ToString()
                + questSO.QuestRequirements[i].ItemData.displayName);
            requirementSlots.Add(questRequirementSlotInstance);
        }

        // Display quest rewards
        for (int i = 0; i < questSO.reward.items.Length; i++)
        {
            QuestInformationSlot questRewardSlotInstance =
                Instantiate(questRewardSlot, questRewardsContainer);
            questRewardSlotInstance.DisplayInformation(questSO.reward.items[i].displayName);
            rewardSlots.Add(questRewardSlotInstance);
        }

        // Display money reward
        if(questSO.reward.money > 0)
        {
            QuestInformationSlot _questRewardSlotInstance =
                Instantiate(questRewardSlot, questRewardsContainer);
            _questRewardSlotInstance.DisplayInformation("Money: " + questSO.reward.money.ToString());
            rewardSlots.Add(_questRewardSlotInstance);
        }
    }

    public void CloseQuestUI()
    {
        currentQuest = null;
        currentQuestInteraction = null;
        questUI.SetActive(false); // Deactivate quest UI panel
    }

    public void OnTryCompleteQuest()
    {
        currentQuestInteraction.AcceptQuest(currentQuest); // Attempt to complete the quest
        currentQuest = null;
        currentQuestInteraction = null;
        questUI.SetActive(false); // Deactivate quest UI panel
    }
}