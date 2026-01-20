using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{

    public int money= 0;
    public TextMeshProUGUI moneyText;

    public static MoneyManager instance;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
    }

    private void Update()
    {
        moneyText.text = money.ToString();

        if (Input.GetKeyDown(KeyCode.K))
        {
            AddMoney(100);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            SpendMoney(100);
        }
    }

    public void AddMoney(int amount)
    {
        money += amount;
    }

    public void SpendMoney(int amount)
    {
        money -= amount;
    }

    public bool CanAfford(int amount)
    {
        return money >= amount;
    }

    public void SetMoney(int amount)
    {
        money = amount;
    }

    public int GetMoney()
    {
        return money;
    }

    public void ResetMoney()
    {
        money = 0;
    }

    public void SaveMoney()
    {
        PlayerPrefs.SetInt("Money", money);
    }

    public void LoadMoney()
    {
        money = PlayerPrefs.GetInt("Money", 0);
    }

    public void ResetSave()
    {
        PlayerPrefs.DeleteKey("Money");
    }

    public void OnApplicationQuit()
    {
        SaveMoney();
    }

    public void OnApplicationPause()
    {
        SaveMoney();
    }

    public void OnApplicationFocus()
    {
        SaveMoney();
    }





}
