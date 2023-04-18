using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;

public class PuzzleFocus : NetworkBehaviour
{
    [SerializeField]
    Image PopUp;
    [SerializeField]
    string InteractionTag;
   

    [SerializeField]
    Button InteraactionCanvas = null;
    [SerializeField]
    GameObject Canvas = null;

    [SerializeField]
    GameObject _Camera;

    [SerializeField]
    CinemachineVirtualCamera _CinemaCamera = null;

    [Header("If left empty, the camera focus will go on to \n the one who triggered the interaction.")]
    [SerializeField]
    Transform _ObjectToFocus = null;
    [Header("Check if you want the camera to switch, \n when you reach another point.")]
    [SerializeField]
    bool DoubleTrigger;
    [Header("If second trigger is needed drag it into this \n field.")]
    [SerializeField]
    CustomTriggerListener SecondTrigger = null;
    //authority check
    bool check;

    //interaciton phase check
    bool interacted = false;
    private void Start()
    {
        ObserverListener.Instance.InteractWizard += InteractButton;
        WireUpTriggers();
    }
    private void OnDisable()
    {
        DisableEvents();
        ObserverListener.Instance.InteractWizard -= InteractButton;
    }
    void WireUpTriggers()
    {
        if (!DoubleTrigger) return;
        SecondTrigger.OnTriggerEntered += TriggerEvent;
        SecondTrigger.OnTriggerExited += ExitTriggerEvent;
    }
    void DisableEvents()
    {
        if (!DoubleTrigger) return;
        SecondTrigger.OnTriggerEntered -= TriggerEvent;
        SecondTrigger.OnTriggerExited -= ExitTriggerEvent;
    }
    private void ExitTriggerEvent()
    {
        throw new NotImplementedException();
    }

    private void TriggerEvent()
    {
        _Camera.SetActive(false);              
        if (Canvas == null) return;
        Canvas.SetActive(false);
    }

    private void InteractButton(bool obj)
    {
                
        if (PopUp.gameObject.activeInHierarchy)
        {
            if (!interacted)
            {
                interacted = true;
                StartInteraction();
            }
            else
            {
                interacted = false;
                StopInteraction();
            }
        }        
    }
    void StartInteraction()
    {
        SetUpLookPoint();
        _Camera.SetActive(true);
        if (Canvas != null)
        {
            Canvas.SetActive(true);
        }
        if (InteraactionCanvas == null) return;
        InteraactionCanvas.Select();
    }
    void StopInteraction()
    {       
        _Camera.SetActive(false);
        if (Canvas != null)
        {
            Canvas.SetActive(false);
        }        
    }
    private void OnTriggerEnter(Collider other)
    {
        check = other.GetComponent<NetworkIdentity>().isLocalPlayer;
        if (other.CompareTag(InteractionTag))
        {            
            if (!check) return;
            SetFocusToPlayer(other.transform);
            PopUp.gameObject.SetActive(true);
        }
    }
   
    private void OnTriggerExit(Collider other)
    {
        check = other.GetComponent<NetworkIdentity>().isLocalPlayer;
        if (other.CompareTag(InteractionTag))
        {
            //checks for local player
            if (!check) return;
            //button pops up
            PopUp.gameObject.SetActive(false);
            
            if (DoubleTrigger) return;
            //resets camera to players
            _Camera.SetActive(false);   
            if (Canvas == null) return;
            Canvas.SetActive(false);
        }
    }
  
    void SetUpLookPoint()
    {
        if(_CinemaCamera == null) { return; }
        _CinemaCamera.LookAt = _ObjectToFocus;
    }
    void SetFocusToPlayer(Transform LookPoint)
    {
        if (_ObjectToFocus != null) return;
        _ObjectToFocus = LookPoint;
    }
}
