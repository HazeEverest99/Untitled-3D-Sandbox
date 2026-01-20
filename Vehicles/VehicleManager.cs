using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VehicleManager : MonoBehaviour
{
    public static VehicleManager instance;

    public CinemachineVirtualCamera playerCamera;
    private CinemachineVirtualCamera activeVehicleCamera;
    private int defaultPriority = 10;
    private int activePriority = 20;

    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void EnterVehicle(CinemachineVirtualCamera vehicleCamera)
    {
        if(activeVehicleCamera != null)
        {
            activeVehicleCamera.Priority = defaultPriority;
        }

        vehicleCamera.Priority = activePriority;
        activeVehicleCamera = vehicleCamera;

        playerCamera.Priority = defaultPriority;
    }

    public void ExitVehicle()
    {
        if(activeVehicleCamera != null)
        {
            activeVehicleCamera.Priority = defaultPriority;
            activeVehicleCamera = null;
        }

        playerCamera.Priority = activePriority;
    }
}
