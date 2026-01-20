using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestInformationSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI informationUILabel;


    public void DisplayInformation(string information)
    {
        informationUILabel.text = information;
    }
}