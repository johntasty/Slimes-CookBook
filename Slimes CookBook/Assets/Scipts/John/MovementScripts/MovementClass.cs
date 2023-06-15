using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Class is used to be able to modify it easier on all instances that use movement
 class MovementClass 
{   
    private MovementAttributes MovementVariables;
   
    private Transform player;
    public Transform _Player
    {
        set => player = value;
    }
    bool hosting;
    public bool Hosting
    {
        set => hosting = value;
    }

    bool _OnPlatform = false;
    Vector3 platform;
    public float testingblend;
    public bool OnPlatform
    {
        get => _OnPlatform;
        set => _OnPlatform = value;
    }
    Transform platCurrent;
    Vector3 platCurrentLastPos;
    
   
    //Input Variables
    private Vector2 inputMove;
    private Vector2 inputRotate;
    Vector3 inputMovement;
    Vector3 inputRotateMove;
    //animator hashes
    private int _XAxis;
    private int _YAxis;
    float VectorMagnitude;

    //Gravity    
    bool grounded;
    bool WallHug;
    float tempGrav;
    //velocity Jumping
    private float _Velocity;
    //movement velocity
    public Vector3 _VelocityVec;
    //Isomateric Skew
    Matrix4x4 isoMatrix;
    Collider[] hits = new Collider[1];
    // Since additive scenes are used the server uses different type of physics
    PhysicsScene CurrentScenePhysics;
    public PhysicsScene _CurrentScene
    {
        set => CurrentScenePhysics = value;
    }
    #region Setup
    // Isometric matrix rotation is used, to correctly apply the directions
    public void Setup(MovementAttributes set)
    {
        this.MovementVariables = set;
        tempGrav = MovementVariables._Gravity;
        isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
    }
    // Blend tree hashes, used for driving the animations
    public void SetHashes(string _XAxisName, string _YaxisName)
    {
        _XAxis = Animator.StringToHash(_XAxisName);
        _YAxis = Animator.StringToHash(_YaxisName);
    }
    public void EnableInputs()
    {
        MovementVariables.playerInput.Enable();
    }
    #endregion
    #region Movement
    public void GroundChecker()
    {
        if (hosting)
        {
            // The server collision has a limited amount of functions for physics
            grounded = player.gameObject.scene.GetPhysicsScene().SphereCast(player.position + (Vector3.up * .35f), .15f, -player.up, out RaycastHit hits, .4f, MovementVariables.groundLayer);
          
            return;
        }
       
        grounded = Physics.CheckSphere(player.position + (Vector3.up * 0.4f), 0.4f, MovementVariables.groundLayer);
    }
    public void Gravity()
    {
        if (grounded && _Velocity < 0.0f)
        {
            _Velocity = -1f;
            return;
        }
        _Velocity += tempGrav * MovementVariables._GravityMultiplier * Time.deltaTime;       
    }
    public void ResetGravityVelocity()
    {
        _Velocity = 0;
    }
    // Input system vectors
    public void MovementInputs(Vector2 input)
    {
        inputMove = input;
        inputMovement.x = input.x;
        inputMovement.z = input.y;
    }
    public void RotationInputs(Vector2 input)
    {
        inputRotate = input;      
    }

    public void Rotation()
    {
        if (inputMove == Vector2.zero) return;

        Vector3 skewedMove = isoMatrix.MultiplyPoint3x4(inputMovement);
        inputRotateMove = (player.position + skewedMove) - player.position;
        Quaternion rotation = Quaternion.LookRotation(inputRotateMove, Vector3.up);       
        player.rotation = Quaternion.Slerp(player.rotation, rotation, MovementVariables.TurnSmoothing * Time.deltaTime);

    }
    // Handles the movement of the player
    public void MovementVector()
    {
        VectorMagnitude = inputMove.normalized.magnitude;
        float accel = VectorMagnitude * MovementVariables.speed;

        Vector3 temp_movement =  player.forward;
        
        _VelocityVec = temp_movement * accel;
        // Wallhug used for the slime
        if (!grounded && WallHug)
        {
            _VelocityVec = temp_movement * VectorMagnitude * 1f;
        }        
        _VelocityVec.y = _Velocity;        
        MovementVariables.playerChar.Move(_VelocityVec * Time.deltaTime);
        // Injects platforms directions to the player movemnts to avoid parenting issues
        if (_OnPlatform) player.position += CalculateOffset();
    }
    // This are called from the curently colliding platform
    public void VelocityInjection(Vector3 PlatformDirection)
    {
        platform = PlatformDirection;
    }
    // This are called from the curently colliding platform
    public void PlatformInjection(Transform PlatformDirection)
    {
        platCurrent = PlatformDirection;
        platCurrentLastPos = PlatformDirection.position;
    }
    Vector3 CalculateOffset()
    {
        Vector3 offset = platCurrent.position - platCurrentLastPos;        
        platCurrentLastPos = platCurrent.position;
        return offset;
    }
    public void ApplyMovement()
    {
        ApplyHashes();
        MovementVector();
    }
    public void Jump()
    {
        if (!grounded) return;

        _Velocity += MovementVariables._JumpPower;
    }
    // Slope matching is not used, since its not visible from an isometric perspective
    public void SlopeMatch()
    {
        RaycastHit hit;
        Physics.Raycast(player.position, -player.up , out hit, MovementVariables.groundLayer);

        Vector3 rotationAxis = Vector3.Cross(player.up, hit.normal).normalized;
        float rotationAngle = Mathf.Acos(Vector3.Dot(player.up, hit.normal)) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
        Quaternion targetRotation = rotation * player.rotation;

        // Smoothly rotate the character to the target rotation
        player.rotation = Quaternion.Lerp(player.rotation, targetRotation, .1f);
    }
    public void WallCheck()
    {
        if (!grounded && WallHug)
        {
            _Velocity = Mathf.Clamp(_Velocity, 0, 0.8f);

        }
        if (hosting)
        {
            int num = CurrentScenePhysics.OverlapSphere(player.position + (Vector3.up * .2f), .3f, hits, MovementVariables.WallLayer, QueryTriggerInteraction.Ignore);
            WallHug = num.Equals(1);          
            return;

        }
        WallHug = Physics.CheckSphere(player.position, MovementVariables.radius, MovementVariables.WallLayer);
    }

    public void TriggerWallStick()
    {
        if (!grounded && WallHug)
        {
            _Velocity = Mathf.Clamp(_Velocity, 0, 0.5f);                       
        }
    }
    #endregion
    #region AnimationApply

    public void ApplyHashes()
    {
        if (!MovementVariables.HasAnimation) return;
        if (!grounded) return;
        float spe = new Vector3(_VelocityVec.x, 0 , _VelocityVec.z).magnitude * VectorMagnitude;
      
        MovementVariables._ControllerAnimator.SetFloat(_YAxis, spe, MovementVariables.AnimationBlendSpd, Time.fixedDeltaTime);
        MovementVariables._ControllerAnimator.SetFloat(_XAxis, inputMove.x, MovementVariables.AnimationBlendTurn, Time.fixedDeltaTime);
    }
    #endregion
}
