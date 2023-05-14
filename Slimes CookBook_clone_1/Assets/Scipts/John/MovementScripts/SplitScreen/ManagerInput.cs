using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using UnityEngine.InputSystem.Users;
public class ManagerInput : MonoBehaviour
{
    public List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField]
    List<Transform> startPoints = new List<Transform>();
  
    [SerializeField]
    private List<LayerMask> playerLayers;
    private PlayerInputManager playerInputManager;


    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player)
    {
       
        players.Add(player);

        //need to use the parent due to the structure of the prefab
        Transform playerParent = player.transform;
       
        player.transform.position = startPoints[players.Count - 1].position;
       
        //convert layer mask (bit) to an integer 
        int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);

        ////set the layer
        playerParent.GetComponentInChildren<CinemachineFreeLook>().gameObject.layer = layerToAdd;
        ////add the layer
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;
        //set the action in the custom cinemachine Input Handler
       
        playerParent.GetComponentInChildren<CinemachineInput>().look = player.actions.FindAction("Look");
    }
}
