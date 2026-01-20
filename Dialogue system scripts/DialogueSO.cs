using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class DialogueSO : ScriptableObject
{
    [System.Serializable]
    public struct DialogueEntry
    {
        [TextArea(5,10)]
        public string npcLine;
        public PlayerResponse[] PlayerResponses;
    }

    public DialogueEntry[] dialogueEntries;
}

[System.Serializable]
public class PlayerResponse
{
    public string PlayerResponseString;

    [Header("If left NULL then the dialogue will continue")]
    public DialogueSO DialogueToTrigger;

    [Header("For quests")]
    public bool isChoiceForQuest = false;
    public QuestSO QuestToTrigger = null;
    public DialogueSO DialogueToTriggerIfCantCompleteQuest;
    public DialogueSO DialogueToTriggerIfCompletedQuest;

    public DialogueActionType action = DialogueActionType.None;
}

public enum DialogueActionType
{
    None,
    OpenShopBuy,
    OpenShopSell,
    LeaveDialogue
}