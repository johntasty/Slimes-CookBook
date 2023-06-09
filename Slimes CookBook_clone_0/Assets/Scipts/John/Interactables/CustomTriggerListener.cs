using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class CustomTriggerListener : NetworkBehaviour
{
    public event Action OnTriggerEntered;
    public event Action OnTriggerExited;

    [Header("What objects to interact with, \n use objects tag.")]
    [SerializeField]
    string InteractionTag;
    [SerializeField]
    bool DontInvokeEvents = false;
    [SerializeField]
    Transform tp;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(InteractionTag))
        {
            if (other.CompareTag("Slime"))
            {
                other.transform.parent.gameObject.SetActive(false);

                other.transform.parent.position = tp.position;

                foreach (Transform child in other.transform.parent)
                {
                    child.position = tp.position;
                }
                other.transform.parent.gameObject.SetActive(true);
            }
            if (DontInvokeEvents)
            {
                OnTriggerEntered?.Invoke();
            }
            if(other.TryGetComponent(out NetworkIdentity networkidentity))
            {
                cmdChangeScene(networkidentity);
            }
            
        }       
    }
    [Command(requiresAuthority = false)]
    void cmdChangeScene(NetworkIdentity networkidentity)
    {
        //NetworkConnectionToClient conn = networkidentity.connectionToClient;
        //conn.Send(new SceneMessage { sceneName = "MainHubScene", sceneOperation = SceneOperation.LoadAdditive, customHandling = false });
        //RoomManagment.singleton.SceneChangeCommand("MainHubScene");
        //RoomManagment.singleton.MovePlayers(networkidentity);
    }
}
