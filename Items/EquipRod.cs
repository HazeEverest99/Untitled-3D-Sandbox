using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EquipRod : Equip
{
    public float castRate;
    public float castRange;
    public bool isCasted = false;
    public bool isPulling = false;

    public Animator anim;
    public GameObject bobberPrefab;
    Transform bobberPosition;
    Camera cam;

    private Transform bobberAlert;

    GameObject bobberReference;

    public bool isFishingAvailable = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        cam = Camera.main;
    }

    private void Update()
    {
            if (isCasted && Input.GetMouseButtonDown(1) && FishingSystem.Instance.isThereABite)
            {
                PullRod();
            }

            if (FishingSystem.Instance.isThereABite && bobberAlert != null)
            {
                bobberAlert.gameObject.SetActive(true);
            }
    }

    private void OnEnable()
    {
        // Subscribe to the event
        FishingSystem.OnEndFishing += EndFishing;

    }

    private void OnDestroy()
    {
        // Unsubscribe to the event
        FishingSystem.OnEndFishing -= EndFishing;

    }

    public void EndFishing()
    {
        if (bobberReference != null)
        {
            Destroy(bobberReference);
        }

        isCasted = false;
        isPulling = false;
        anim.ResetTrigger("Cast");

    }

    public override void UseEquip()
    {

            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Water"))
                {
                    isFishingAvailable = true;

                    if (!isCasted && !isPulling)
                    {
                        StartCoroutine(CastRod(hit.point));
                    }
                }
                else isFishingAvailable = false;
            }

            else isFishingAvailable = false;

    }

    public override void AltUseEquip()
    {

    }

    IEnumerator CastRod(Vector3 targetPosition)
    {
        isCasted = true;
        anim.SetTrigger("Cast");

        yield return new WaitForSeconds(1f);

        bobberReference = Instantiate(bobberPrefab, targetPosition, Quaternion.identity);
        bobberAlert = bobberReference.transform.Find("Alert");
        if (bobberAlert != null) bobberAlert.gameObject.SetActive(false);

        FishingSystem.Instance.StartFishing(FishingLocation.Lake);
    }

    public void PullRod()
    {
        isPulling = true;
        isCasted = false;
        anim.SetTrigger("Pull");

        if (bobberReference != null && bobberReference.transform.Find("Alert") != null)
        {
            bobberReference.transform.Find("Alert").gameObject.SetActive(false);
        }


        Debug.Log("Pulling Rod");

        FishingSystem.Instance.SetHasPulled();
    }
}