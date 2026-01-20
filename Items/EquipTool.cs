using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    public float toolGatherRate;
    private bool toolGathering = false;
    public float toolGatherDistance;

    [Header("Resource Gathering")]
    public bool canGather;

    public Animator animator;
    private Camera cam;

    void Awake ()
    {
        animator = GetComponent<Animator>();
        cam = Camera.main;
    }
    public override void UseEquip()
    {
        if (!toolGathering)
        {
            toolGathering = true;
            animator.SetTrigger("Gathering");
            Invoke("GatherResource", toolGatherRate);
        }
    }

    void GatherResource()
    {
        toolGathering = false;
    }

    public override void AltUseEquip()
    {

    }

    public void OnHit()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, toolGatherDistance))
        {
            if(canGather && hit.collider.GetComponent<ResourceTree>())
            {
                hit.collider.GetComponent<ResourceTree>().Gather(hit.point, hit.normal);
            }
        }
    }
}
