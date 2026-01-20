using System.Collections;
using System.Collections.Generic;
using Ashsvp;
using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class VehicleObject : MonoBehaviour, IInteractable
{
    public VehicleData vehicleData;
    public GameObject player;
    public GameObject carExit;
    public FirstPersonMovement playerMove;
    public SimcadeVehicleController vehicleController;
    public CinemachineVirtualCamera vehicleCamera;

    public bool isDriving = false;
    
    public void OnInteract()
    {
        EnterVehicle();
    }

    public string GetInteractPrompt()
    {
        return string.Format("Enter {0}", vehicleData.displayName);
    }

    public void EnterVehicle()
    {
        VehicleManager.instance.EnterVehicle(vehicleCamera);

        player.SetActive(false);
        vehicleController.CanDrive = true;
        vehicleController.CanAccelerate = true;
        isDriving = true;

        playerMove.DisableColliderAndRigidbody();
    }

    public void ExitVehicle()
    {
        VehicleManager.instance.ExitVehicle();

        player.transform.position = carExit.transform.position;
        player.SetActive(true);
        vehicleController.CanDrive = false;
        vehicleController.CanAccelerate = false;
        isDriving = false;

        playerMove.EnableColliderAndRigidbody();
    }
}
