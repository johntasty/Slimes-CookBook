
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

    bool Respawing = false;
    CharacterController playerChar;
    public override void OnStartAuthority()
    {
        _PlayerCam.enabled = true;
        _Camera.enabled = true;        

        PlayerInput player = GetComponent<PlayerInput>();
        
        //transform.GetComponentInChildren<CinemachineInput>().look = player.actions.FindAction("Look");
    }
    private void Start()
    {
        playerChar = GetComponentInChildren<CharacterController>();
    }
    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;
        if (playerChar.transform.position.y < -60)
        {
            CmdAskSceneName();
        }
    }
    [Command(requiresAuthority = false)]
    void CmdAskSceneName()
    {
        RespawnPlayer();
        RpcCommandRespawn();
    }

    [ClientRpc]
    void RpcCommandRespawn()
    {
        RespawnPlayer();
    }
    void RespawnPlayer()
    {
        if (Respawing) return;
        StartCoroutine(RespawnTimer());
    }

    IEnumerator RespawnTimer()
    {
        if (TryGetComponent(out NetworkIdentity identity))
        {
            Respawing = true;
            if (isLocalPlayer)
            {
                yield return StartCoroutine(AdditiveNetwork.singleton.fadeInOut.FadeIn());
            }
            yield return new WaitForSeconds(AdditiveNetwork.singleton.fadeInOut.GetDuration());

            Vector3 respawnPosition = Vector3.zero;//AdditiveNetwork.singleton.GetTeleportPosition(gameObject.scene.name).position;
            float dist = 200;
            foreach (KeyValuePair<string, Transform> item in AdditiveNetwork.teleportRegistar)
            {
                float curDist = Vector3.Distance(transform.position, item.Value.position);
                Debug.Log(curDist);
                if (curDist < dist)
                {
                    respawnPosition = item.Value.position;
                }
            }
            transform.gameObject.SetActive(false);

            transform.position = respawnPosition;

            foreach (Transform child in transform)
            {
                child.position = respawnPosition;
            }
            transform.gameObject.SetActive(true);
            Respawing = false;
            if (isLocalPlayer)
            {
                yield return StartCoroutine(AdditiveNetwork.singleton.fadeInOut.FadeOut());
            }
            yield return new WaitForSeconds(AdditiveNetwork.singleton.fadeInOut.GetDuration());
        }
    }
}
