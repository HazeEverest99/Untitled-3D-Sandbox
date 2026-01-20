using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using Unity.VisualScripting;
using System.Security.Cryptography;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager instance;
    public float checkRate = 0.05f;
    private float lastCheckTime;
    public float maxCheckDistance;
    public LayerMask layerMask;
    public GameObject curInteractGameObject;
    private IInteractable curInteractable;
    public TextMeshProUGUI promptText;
    public Camera cam;
    public VehicleObject vehicleObject;
    

    private bool isButtonHeld = false;
    private float holdStartTime = 0f;
    private float holdDuration = .5f; // .5 seconds

    void Start ()
    {
        instance = this;
        cam = Camera.main;
    }
    void Update()
    {
        // true every "checkRate" seconds
        if(Time.time - lastCheckTime > checkRate)
        {
            lastCheckTime = Time.time;
            // create a ray from the center of our screen pointing in the direction we're looking
            Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
            RaycastHit hit;
            // did we hit something?
            if(Physics.Raycast(ray, out hit, maxCheckDistance, layerMask))
            {
                // is this not our current interactable?
                // if so, set it as our current interactable
                if(hit.collider.gameObject != curInteractGameObject)
                {
                    curInteractGameObject = hit.collider.gameObject;
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    SetPromptText();
                }
            }
            else
            {
                curInteractGameObject = null;
                curInteractable = null;
                promptText.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && curInteractable != null)
        {          
            curInteractable.OnInteract();
            curInteractGameObject = null;
            curInteractable = null;
            promptText.gameObject.SetActive(false);      
        }


        if (vehicleObject != null && vehicleObject.isDriving && Input.GetKeyDown(KeyCode.E))
        {
            vehicleObject.ExitVehicle();
        }

    }
    // called when we hover over a new interactable
    void SetPromptText ()
    {
        if (curInteractable != null)
        {
        promptText.gameObject.SetActive(true);
        promptText.text = string.Format("<b>[E]</b> {0}", curInteractable.GetInteractPrompt());
        }
    }

    void OnDrawGizmos()
    {
        Ray ray = cam.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        Gizmos.DrawRay(ray.origin, ray.direction * maxCheckDistance);
    

    }
}

public interface IInteractable
{
    string GetInteractPrompt();
    void OnInteract();
}