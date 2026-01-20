using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public enum FishingLocation
{
    Lake,
    River,
    Ocean
}

public class FishingSystem : MonoBehaviour
{
    public static FishingSystem Instance { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public List<FishData> lakeFishList;
    public List<FishData> riverFishList;
    public List<FishData> oceanFishList;

    public FishData currentFish;

    public bool isThereABite;
    public bool hasPulled;

    public static event Action OnEndFishing;
    private bool isFishCalculated;
    private Coroutine fishingRoutine;

    internal void StartFishing(FishingLocation location)
    {
        if(fishingRoutine != null)
        {
            StopCoroutine(fishingRoutine);
        }
        fishingRoutine = StartCoroutine(FishingCoroutine(location));
    }

    IEnumerator FishingCoroutine(FishingLocation location)
    {
        isFishCalculated = false;

        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 15));

        if (!isFishCalculated)
        {
            currentFish = CalculateBite(location);
            isFishCalculated = true;
        }

        if (currentFish.fishName == "NoBite")
        {
            Debug.Log("Not even a nibble");
            OnFishingFailure();
        }
        else
        {
            Debug.Log("A " + currentFish.fishName + " is biting!");
            StartCoroutine(StartFishStruggle(currentFish));
        }
    }

    IEnumerator StartFishStruggle(FishData fish)
    {
        isThereABite = true;
        hasPulled = false;

        float timeWindow = UnityEngine.Random.Range(1f, 5f);
        float t = 0f;

        while (t < timeWindow)
        {
            if (hasPulled)
            {
                OnFishingSuccess();
                yield break; // exit coroutine if player has pulled
            }

            t += Time.deltaTime;
            yield return null;
        }
        // if player doesn't pull the rod in time, the fish will escape

        OnFishingFailure();
    }

    public void SetHasPulled()
    {
        hasPulled = true;
    }

    private void OnFishingSuccess()
    {
        isThereABite = false;
        hasPulled = false;


        // Trigger End Fishing Event
        OnEndFishing?.Invoke();

        
        var slot = InventoryManager.instance.currentHotbarIndex;

        InventoryManager.instance.SelectSlot(slot);
        InventoryManager.instance.SelectSlot(slot); 
        
        if (currentFish != null && currentFish.itemData != null)
        {
            InventoryManager.instance.AddItem(currentFish.itemData, 1);
        }
    }

    private void OnFishingFailure()
    {
        isThereABite = false;
        hasPulled = false;


        // Trigger End Fishing Event
        OnEndFishing?.Invoke();
    }

    private FishData CalculateBite(FishingLocation location)
    {
        List<FishData> fishList = GetAvailableFish(location);

        //calculate the probability of each fish biting
        float totalProbability = 0f;
        foreach (FishData fish in fishList) // if bluegill has 20 (0-19) and carp has 35 (20-54) then totalProbability = 55
        {
            totalProbability += fish.probability;
            Debug.Log("Total Probability: " + totalProbability);
        }

        //Generate a random number between 0 and totalProbability
        int randomNumber = UnityEngine.Random.Range(0, Mathf.FloorToInt(totalProbability) + 1); // if totalProbability = 55 then randomNumber = 0 to 55
        Debug.Log("Random number: " + randomNumber);

        //Loop through the fish list and check if the random number is less than the fish's probability
        float cumulativeProbability = 0f;
        foreach (FishData fish in fishList)
        {
            cumulativeProbability += fish.probability;
            if (randomNumber <= cumulativeProbability)
            {
                return fish;
            }
        }

        //This should never happen - Random number out of bounds
        return null;
    }

    private List<FishData> GetAvailableFish(FishingLocation location)
    {
        switch (location)
        {
            case FishingLocation.Lake:
                return lakeFishList;
            case FishingLocation.River:
                return riverFishList;
            case FishingLocation.Ocean:
                return oceanFishList;
            default:
                return null;
        }
    }

}
