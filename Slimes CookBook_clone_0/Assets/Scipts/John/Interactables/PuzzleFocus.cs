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
    CinemachineVirtualCamera _CinemaCamera = null;

    [Header("If left empty, the camera focus will go on to \n the one who triggered the interaction.")]
    [SerializeField]
    Transform _ObjectToFocus = null;
    [Header("Check if you want the camera to switch, \n when you reach another point.")]
    [SerializeField]
    bool DoubleTrigger;
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
    void StartInteraction()
    {
        SetUpLookPoint();
        if(TeleportPoint != null) TeleportPlayer(TeleportPoint.position);

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
        
        if (other.TryGetComponent(out NetworkIdentity compomn))
        {
            check = compomn.isLocalPlayer;
            
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
        if(other.TryGetComponent(out NetworkIdentity compomn))
        {
            check = compomn.isLocalPlayer;
           
        }
        else { check = false; }
       
        if (other.CompareTag(InteractionTag))
        {
           
            //checks for local player
            if (!check) return;
           
            //button pops up
            PopUp.gameObject.SetActive(false);
            interactor =null;
            if (DoubleTrigger) return;
            //resets camera to players
            _Camera.SetActive(false);   
            if (Canvas == null) return;
            Canvas.SetActive(false);
        }
    }
  
    void SetUpLookPoint()
    {
       
        if (_CinemaCamera == null) { return; }
       
        _CinemaCamera.LookAt = _ObjectToFocus;
       
    }
    void TeleportPlayer(Vector3 teleportPoint)
    {
        if (Teleport)
        {
            interactor.gameObject.SetActive(false);
            interactor.position = teleportPoint;
            interactor.gameObject.SetActive(true);
        }
    }
    void SetFocusToPlayer(Transform LookPoint)
    {
       
        if (_ObjectToFocus != null) return;

        _ObjectToFocus = LookPoint;
        
    }

    [Command(requiresAuthority = false)]
    void Testing(NetworkIdentity networkIdentity)
    {
        //NetworkConnectionToClient conn = networkIdentity.connectionToClient;
        //conn.Send(new SceneMessage { sceneName = "Room_1_WallPuzzle", sceneOperation = SceneOperation.LoadAdditive, customHandling = false });
        RpcLoadUnloadScene("Room_1_WallPuzzle", LoadAction.Load);

    }
    [ClientRpc]
    public void RpcLoadUnloadScene(string SceneName, LoadAction loadAction)
    {
        StartCoroutine(LoadUnloadScene(SceneName, loadAction));
    }
    private IEnumerator LoadUnloadScene(string sceneName, LoadAction loadAction)
    {       

        if (loadAction == LoadAction.Load)
        {
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            
        }
        else
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets();
        }

        Debug.Log("checking");
        // Tell mirror to update the list of available objects
        NetworkClient.PrepareToSpawnSceneObjects();
        
        Debug.LogFormat("{0} {1} Done", sceneName, loadAction.ToString());

    }
    [Command(requiresAuthority = false)]
    void UnloadScene(NetworkIdentity networkIdentity)
    {
        RpcLoadUnloadScene("Room_1_WallPuzzle", LoadAction.Unload);
        //NetworkConnectionToClient conn = networkIdentity.connectionToClient;
        //conn.Send(new SceneMessage { sceneName = "Room_1_WallPuzzle", sceneOperation = SceneOperation.UnloadAdditive, customHandling = false });

    }
}
