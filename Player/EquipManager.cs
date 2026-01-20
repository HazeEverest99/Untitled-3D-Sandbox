using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer.Internal;
using UnityEngine;
public class EquipManager : MonoBehaviour
{
    public Equip curEquip;
    public Transform equipParent;
    private FirstPersonLook playerLook;

    //singleton
    public static EquipManager instance;


    void Awake ()
    {
        instance = this;
        playerLook = FindObjectOfType<FirstPersonLook>();
    }

    public void Update ()
    {
        if (Input.GetMouseButtonDown(0) && curEquip != null && playerLook != null && playerLook.canLook)
            UseEquip();
        if (Input.GetMouseButtonDown(1) && curEquip != null && playerLook != null && playerLook.canLook)
            AltUseEquip();
    }

    public void UseEquip ()
    {
            curEquip.UseEquip();
    }

    public void AltUseEquip ()
    {
            curEquip.AltUseEquip();
    }

    public void EquipNew (ItemData item)
    {
        UnEquip();
        curEquip = Instantiate(item.equipPrefab, equipParent).GetComponent<Equip>();

        var rod = curEquip as EquipRod;
        if (rod != null && rod.anim != null)
        {
            rod.anim.ResetTrigger("Cast");
            rod.anim.ResetTrigger("Pull");
        }
    }

    public void UnEquip ()
    {
        if (curEquip != null)
        {

            if(curEquip is EquipRod rod)
            {
                rod.EndFishing();
                Debug.Log("Unequip Ended Fishing");
            }

            Destroy(curEquip.gameObject);
            curEquip = null;
            Debug.Log("Unequipped");

        }


    }
}
