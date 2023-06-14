
using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class CameraAuthority : NetworkBehaviour
{
    [SerializeField]
    CinemachineVirtualCamera _Camera;
    [SerializeField]
    Camera _PlayerCam;

    public override void OnStartAuthority()
    {
        _PlayerCam.enabled = true;
        _Camera.enabled = true;        

    }
       
}
