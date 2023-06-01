using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTriggers : MonoBehaviour
{
    public static Action<Vector3> TriggerPosition;
    bool interactingCollider = false;
    [SerializeField]
    ElevatorButtons sesawManager;

    Transform player = null;
    private void OnEnable()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;
       
    }

    private void InteractButton(bool obj)
    {
        if (!interactingCollider) return;
        if (!sesawManager.CheckLocal(player)) { return; }
        Vector3 position = transform.position;
        TriggerPosition?.Invoke(position);
     
    }

    private void OnDestroy()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            interactingCollider = true;
            player = other.transform.parent;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            interactingCollider = false;
            player = null;
        }
    }
}
