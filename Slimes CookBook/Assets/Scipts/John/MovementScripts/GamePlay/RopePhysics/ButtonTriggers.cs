using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTriggers : MonoBehaviour
{
    public static Action<Vector3, String> TriggerPosition;
    bool interactingCollider = false;
    [SerializeField]
    ElevatorButtons sesawManager;
    [SerializeField]
    string TagToInteract;
    Transform player = null;
    [SerializeField]
    GameObject popUp;
    private void OnEnable()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;       
    }

    private void InteractButton(bool obj)
    {
        if (!interactingCollider) return;
        if (!sesawManager.CheckLocal(player)) { return; }
        Vector3 position = transform.position;
        TriggerPosition?.Invoke(position, gameObject.name);
     
    }

    private void OnDestroy()
    {
        ObserverListener.Instance.InteractWizard -= InteractButton;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(TagToInteract))
        {
            interactingCollider = true;
            player = other.transform.parent;
            popUp.SetActive(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(TagToInteract))
        {
            interactingCollider = false;
            player = null;
            popUp.SetActive(false);
        }
    }
}
