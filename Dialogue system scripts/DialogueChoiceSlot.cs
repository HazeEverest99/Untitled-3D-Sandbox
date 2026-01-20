using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueChoiceSlot : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI choiceUILabel = null;

    private PlayerResponse playerResponse = null;

    private void Start()
    {
        button.onClick.AddListener(SubmitAnswer);
    }

    public void SetData(PlayerResponse playerResponse)
    {
        choiceUILabel.text = playerResponse.PlayerResponseString;
        this.playerResponse = playerResponse;
    }

    public void SubmitAnswer()
    {
        DialogueManager.instance.OnSubmitAnswer(playerResponse);
    }
}