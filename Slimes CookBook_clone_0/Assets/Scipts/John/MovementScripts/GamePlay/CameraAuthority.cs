
using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.InputSystem;
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

        PlayerInput player = GetComponent<PlayerInput>();
        //transform.GetComponentInChildren<CinemachineInput>().look = player.actions.FindAction("Look");
    }
}
