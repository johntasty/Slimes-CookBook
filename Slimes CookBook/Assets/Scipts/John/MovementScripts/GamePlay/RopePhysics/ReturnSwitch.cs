using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnSwitch : MonoBehaviour
{
    [SerializeField]
    ElevatorButtons sesawManager;
    public int playersOn = 0;
    private void OnEnable()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;
    }
    private void OnDisable()
    {
        playersOn = 0;
    }
    private void InteractButton(bool obj)
    {
        if (playersOn < 2) return;
        sesawManager.CmdReturnJourney();
    }

    private void OnDestroy()
    {
        ObserverListener.Instance.InteractWizard -= InteractButton;
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            playersOn++;            
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            playersOn--;            
        }
        
    }
}
