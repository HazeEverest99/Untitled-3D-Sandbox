using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest")]
public class QuestSO : ScriptableObject
{
    public string QuestName;

    [TextArea(10,12)]
    public string QuestDescription;
    public QuestRequirement[] QuestRequirements;
    public Reward reward;

    public DialogueSO QuestDialogue;
}

[System.Serializable]
public class QuestRequirement
{
    public ItemData ItemData;
    public int ItemQuantityNeeded = 0;
}

[System.Serializable]
public class Reward
{
    public ItemData[] items;
    public int money;
}