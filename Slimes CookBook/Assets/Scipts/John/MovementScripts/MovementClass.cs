using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 class MovementClass 
{   
    private MovementAttributes MovementVariables;

    private Transform player;
    public Transform _Player
    {
        set => player = value;
    }

    //turn variables
    float _TargetAngle;
    float turnSmoothVel;

    //Input Variables
    private Vector2 inputMove;
    private Vector2 inputRotate;

    //animator hashes
    private int _XAxis;
    private int _YAxis;

    //Gravity    
    bool grounded;
    bool WallHug;
    float tempGrav;
    //velocity Jumping
    private float _Velocity;
    //movement velocity
    Vector3 _VelocityVec;

    #region Setup
    public void Setup(MovementAttributes set)
    {
        this.MovementVariables = set;
        tempGrav = MovementVariables._Gravity;
    }
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
        grounded = Physics.CheckSphere(player.position, 0.3f, MovementVariables.groundLayer);
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

    public void MovementInputs(Vector2 input)
    {
        inputMove = input;
    }
    public void RotationInputs(Vector2 input)
    {
        inputRotate = input;
    }

    public void Rotation()
    {
        _TargetAngle = Mathf.Atan2(inputMove.x, inputMove.y) * Mathf.Rad2Deg + MovementVariables.cam.eulerAngles.y;

        float angle = Mathf.SmoothDampAngle(player.eulerAngles.y, _TargetAngle, ref turnSmoothVel, MovementVariables.turn);
        Vector3 playRot = player.rotation.eulerAngles;
        player.rotation = Quaternion.Euler(playRot.x, angle, playRot.z);
    }

    public void MovementVector()
    {
        float VectorMagnitude = inputMove.magnitude;
        float accel = VectorMagnitude * MovementVariables.speed;

        Vector3 temp_movement = Quaternion.Euler(0f, _TargetAngle, 0f) * Vector3.forward;

        _VelocityVec = temp_movement * VectorMagnitude * MovementVariables.speed;
        _VelocityVec.y = _Velocity;
    }
    public void ApplyMovement()
    {
        MovementVector();
        MovementVariables.playerChar.Move(_VelocityVec * Time.deltaTime);
        ApplyHashes();
    }
    public void Jump()
    {
        if (!grounded) return;
        _Velocity += MovementVariables._JumpPower;
    }
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
        
        WallHug = Physics.CheckSphere(player.position, 0.3f, MovementVariables.WallLayer);
        //if (!WallHug) { tempGrav = MovementVariables._Gravity; }
        if (!grounded && WallHug)
        {            
            _Velocity = Mathf.Clamp(_Velocity, 0, 0.5f);

            //Vector3 diff = player.position - hit.normal;
            //float angle = Mathf.Atan2(diff.x, diff.y) * Mathf.Rad2Deg;
            //Vector3 playRot = player.rotation.eulerAngles;           
            //player.rotation = Quaternion.Euler(angle * 5, playRot.y, playRot.z);
        }
    }
    #endregion
    #region AnimationApply

    public void ApplyHashes()
    {
        if (!MovementVariables.HasAnimation) return;
        if (!grounded) return;
        float spe = _VelocityVec.magnitude;
        MovementVariables._ControllerAnimator.SetFloat(_YAxis, spe, MovementVariables.AnimationBlendSpd, Time.fixedDeltaTime);
        MovementVariables._ControllerAnimator.SetFloat(_XAxis, inputRotate.x, MovementVariables.AnimationBlendTurn, Time.fixedDeltaTime);
    }
    #endregion
}
