using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
using System.Collections;
using System.Collections.Generic;
public class SimpleControll : NetworkBehaviour
{
    [SerializeField] Camera _PlayerCam;

    [Header("Attributes")]
    [SerializeField] MovementAttributes SetInputs;
    [SerializeField] bool SlimeMovement;
    [SerializeField] Material SlimeMat = null;
    static readonly int _SlimeHit = Shader.PropertyToID("_Collision");
    private MovementClass MovementFunctions;
   
    public bool WallHug;
    public bool GravityApply = true;
  
    public event Action<Vector2> Look;
    public event Action<bool> Interact;

    CharacterController character = null;
    bool Respawing = false;

   
    public float radi;
    public float dista;
    public override void OnStartAuthority()
    {
       
        enabled = true;
       
        SetAttributes();
        if (isServer)
        {
            HostingPlayer();
        }
        if (SlimeMovement)
        {
            ObserverListener.Instance._Slime = GetComponent<SimpleControll>();
            ObserverListener.Instance.WireUpSlime();
            return;
        }
        ObserverListener.Instance._Wizard = GetComponent<SimpleControll>();
        ObserverListener.Instance.WireUp();
    }
   
    public void HostingPlayer()
    {
        MovementFunctions.Hosting = true;
       
    }
    public override void OnStopAuthority()
    {
        this.enabled = false;        
    }
    private void SetAttributes()
    {
       
        GetComponent<PlayerInput>().enabled = true;
        MovementFunctions = new MovementClass();

        SlimesCookBook playerInput = new SlimesCookBook();
        character = GetComponentInChildren<CharacterController>();
        MovementFunctions._Player = character.transform;

        SetInputs.playerInput = playerInput;
        SetInputs.playerChar = character;
        if (SetInputs.HasAnimation) { 
            SetInputs._ControllerAnimator = GetComponentInChildren<Animator>();
            SetInputs._ControllerAnimator.enabled = true;
            MovementFunctions.SetHashes("XAxis", "YAxis");           
        }
        
        MovementFunctions.Setup(SetInputs);
        MovementFunctions.EnableInputs();
        SetInputs.playerInput.Enable();
    }
   
    private void FixedUpdate()
    {
       
        if (!isLocalPlayer) return;
        MovementFunctions.GroundChecker();
        if (SlimeMovement)
        {
            MovementFunctions.WallCheck();
        }
        
        MovementFunctions.Gravity();

        if (character.transform.position.y < -60)
        {
            RespawnPlayer();
        }
        MovementFunctions.Rotation();
        MovementFunctions.ApplyMovement();
       

    }

    private void LateUpdate()
    {
                
        if (!isLocalPlayer) return;
      
        
       
    }

    private void OnEnable()
    {
        StartCoroutine(PhysicsSetup());
    }
    IEnumerator PhysicsSetup()
    {
        while(MovementFunctions == null)
        {
            yield return null;
        }
        MovementFunctions._CurrentScene = gameObject.scene.GetPhysicsScene();
    }
    public void OnPlatform()
    {
        MovementFunctions.OnPlatform = !MovementFunctions.OnPlatform;
    }
    public void InjectDirection(Vector3 platform)
    {       
        MovementFunctions.VelocityInjection(platform);
    }
    public void InjectPosition(Transform platform)
    {
        MovementFunctions.PlatformInjection(platform);
    }
    private void OnDisable()
    {
               
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        MovementFunctions.MovementInputs(context.ReadValue<Vector2>());
               
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        MovementFunctions.RotationInputs(context.ReadValue<Vector2>().normalized);

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Interact?.Invoke(true);
        }
        
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (!context.started) return;
        MovementFunctions.Jump();
            
    }
    void JumpNow()
    {
      //ToDo

    }
    void Peaked()
    {
        //ToDo
    }
    #region ServerRespawn
      
    void RespawnPlayer()
    {
        if (Respawing) return;      
        StartCoroutine(RespawnTimer());
    }
    IEnumerator RespawnTimer()
    {
        Respawing = true;
        float timer = AdditiveNetwork.singleton.fadeInOut.GetDuration();
        if (isLocalPlayer)
        {
            yield return StartCoroutine(AdditiveNetwork.singleton.fadeInOut.FadeIn());
        }
        yield return new WaitForSeconds(timer);


        Transform respawnPosition = null;//AdditiveNetwork.singleton.GetTeleportPosition(gameObject.scene.name).position;
        float dist = 1000;

        foreach (KeyValuePair<string, Transform> item in AdditiveNetwork.teleportRegistar)
        {
            float curDist = Vector3.Distance(character.transform.position, item.Value.position);
           
            if (curDist < dist)
            {
                dist = curDist;
                respawnPosition = item.Value;
            }
        }

        transform.SetParent(respawnPosition);

        transform.localPosition = Vector3.zero;
        if (isLocalPlayer)
        {
            MovementFunctions.ResetGravityVelocity();
        }        
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
            child.localPosition = Vector3.zero;
            child.gameObject.SetActive(true);

        }

        transform.SetParent(null);
        if (isLocalPlayer)
        {
            yield return StartCoroutine(AdditiveNetwork.singleton.fadeInOut.FadeOut());
        }       
        yield return new WaitForSeconds(timer);

        Respawing = false;
    }
    #endregion



}
