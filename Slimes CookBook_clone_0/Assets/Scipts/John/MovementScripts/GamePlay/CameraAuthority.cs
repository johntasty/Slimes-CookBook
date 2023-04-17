
using UnityEngine;
using Mirror;
using Cinemachine;
using UnityEngine.InputSystem;
public class CameraAuthority : NetworkBehaviour
{
    [SerializeField]
    CinemachineFreeLook _Freelook;
    [SerializeField]
    Camera _PlayerCam;
    public override void OnStartAuthority()
    {
        _PlayerCam.enabled = true;
        _Freelook.enabled = true;        

        PlayerInput player = GetComponent<PlayerInput>();
        transform.GetComponentInChildren<CinemachineInput>().look = player.actions.FindAction("Look");
    }
}
