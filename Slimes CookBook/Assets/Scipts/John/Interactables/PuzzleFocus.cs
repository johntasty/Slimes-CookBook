using Mirror;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class PuzzleFocus : NetworkBehaviour
{
    public enum LoadAction
    {
        Load,
        Unload
    }

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
    Camera _MainCam;

    [SerializeField]
    CinemachineVirtualCamera _CinemaCamera = null;

    [Header("If left empty, the camera focus will go on to \n the one who triggered the interaction.")]
    [SerializeField]
    Transform _ObjectToFocus = null;
    [Header("Check if you want the camera to switch, \n when you reach another point.")]
    [SerializeField]
    bool DoubleTrigger;
    [Header("Check if you want the camera follow to switch, \n when you reach another point.")]
    [SerializeField]
    bool Follow;
    [Header("Slime teleport")]
    [SerializeField]
    bool Teleport;
    [SerializeField]
    Transform TeleportPoint = null;
    [SerializeField]
    Transform TeleportPointBack = null;
    [Header("If second trigger is needed drag it into this \n field.")]
    [SerializeField]
    CustomTriggerListener SecondTrigger = null;
    //authority check
    bool check;

    private Transform interactor;
    //interaciton phase check
    bool interacted = false;
    // Wires up the interactor from the input system
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
        // If the slime Character, some places he goes he should not be able to see the enviroment
        // thus the scene bakground changes
        _MainCam = Camera.main;
        _MainCam.clearFlags = CameraClearFlags.Skybox;
        TeleportPlayer(TeleportPointBack.position);
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
    // The functionallity is made in a way to be reused in all objects
    // Each object that has some interaction and requiers some different cameras or focus uses this script
    void StartInteraction()
    {
        SetUpLookPoint();
        if (TeleportPoint != null) {
            _MainCam = Camera.main;
            _MainCam.clearFlags = CameraClearFlags.SolidColor;
            TeleportPlayer(TeleportPoint.position);
        }

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
        
        if (other.transform.parent != null)
        {
            // Check for local players as to not have the ui pop on both, only on local
            if(other.transform.parent.TryGetComponent(out NetworkIdentity compomn))
            {
                check = compomn.isLocalPlayer;
            }            
            
        }
        else { check = false; }
       
        if (other.CompareTag(InteractionTag))
        {            
            if (!check) return;
            interactor = other.transform;
            SetFocusToPlayer(other.transform);
            PopUp.gameObject.SetActive(true);
        }
    }
   
    private void OnTriggerExit(Collider other)
    {       
       
        if (other.CompareTag(InteractionTag))
        {
           
            //checks for local player
            if (!check) return;
           
            //button pops up
            PopUp.gameObject.SetActive(false);
            interactor = null;
            if (DoubleTrigger) return;
            //resets camera to players
            _Camera.SetActive(false);   
            if (Canvas == null) return;
            Canvas.SetActive(false);
        }
    }
  void SetUpFollow()
    {        
        
        if (Follow)
        {
            _CinemaCamera.Follow = _ObjectToFocus;
        }        
    }
    void SetUpLookPoint()
    {
       
        if (_CinemaCamera == null) { return; }
       
        _CinemaCamera.LookAt = _ObjectToFocus;
        SetUpFollow();


    }
    void TeleportPlayer(Vector3 teleportPoint)
    {
        if (Teleport)
        {
            PopUp.gameObject.SetActive(false);
            interactor.parent.gameObject.SetActive(false);
            if(InteractionTag == "Slime")
            {
                // Slime special case, he is a bunch of balls that use custom physics,
                // Not moving them all individually displaces them to much
                Transform balls = interactor.parent.Find("ParentBall");
                int len = balls.childCount;
                for (int i = 0; i < len - 1; i++)
                {
                    balls.GetChild(i).position = teleportPoint;
                }
            }                
            interactor.position = teleportPoint;           
            interactor.parent.gameObject.SetActive(true);
        }
    }
    void SetFocusToPlayer(Transform LookPoint)
    {
       
        if (_ObjectToFocus != null) return;

        _ObjectToFocus = LookPoint;
        
    }

}
