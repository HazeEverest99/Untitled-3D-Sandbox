using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Vehicle", menuName = "New Vehicle")]
public class VehicleData : ScriptableObject
{
    [Header("Vehicle Info")]
    public string displayName;
}
