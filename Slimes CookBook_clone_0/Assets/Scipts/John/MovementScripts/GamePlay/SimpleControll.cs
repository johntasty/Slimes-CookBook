using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Mirror;
public class SimpleControll : NetworkBehaviour
{
    [SerializeField] Camera _PlayerCam;

    [Header("Attributes")]
    [SerializeField] MovementAttributes SetInputs;
    [SerializeField] bool SlimeMovement;
    [SerializeField] Material SlimeMat = null;
    static readonly int _SlimeHit = Shader.PropertyToID("_Collision");
    private MovementClass MovementFunctions;
    private WallDissolve wallProperties;
    public bool WallHug;
    public bool GravityApply = true;
  
    public event Action<Vector2> Look;
    public event Action<bool> Interact;

   
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
        //wallProperties = GetComponentInChildren<WallDissolve>();
        //wallProperties.enabled = true;
        //wallProperties.WallSetup();
        GetComponent<PlayerInput>().enabled = true;
        MovementFunctions = new MovementClass();

        SlimesCookBook playerInput = new SlimesCookBook();
        CharacterController character = GetComponentInChildren<CharacterController>();
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
        //MovementFunctions.SlopeMatch();
       
    }
    
    private void LateUpdate()
    {
                
        if (!isLocalPlayer) return;
       
        //wallProperties.WallDissolusion();        
        MovementFunctions.Rotation();
        MovementFunctions.ApplyMovement();

    }

    private void OnEnable()
    {

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
        Interact?.Invoke(true);
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!isLocalPlayer) return;
        if (!context.started) return;
        MovementFunctions.Jump();
       // if (!grounded) return;       
       //_Velocity += _JumpPower;
       
    }
    void JumpNow()
    {
      //ToDo

    }
    void Peaked()
    {
        //ToDo
    }
   

}
