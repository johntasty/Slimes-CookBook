using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleControll : MonoBehaviour
{
    private int _id;
    public static int shaderVariableName = Shader.PropertyToID("_Height");
    private Vector3 _distanceObjectToCamera;
    [SerializeField] Camera _PlayerCam;
    [SerializeField] Material _Wall;
    [Header("Move Speed")] 
    [SerializeField] private float speed;
    [SerializeField] private float Maxspeed;
    private Vector3 _Direction;
    private Vector3 movement;
    Vector3 _VelocityVec;
    float _Speed;
   [Header("Gravity Power")]
    [SerializeField] float _GravityMultiplier;
    private float _Velocity;
    private float _Gravity = -9.81f;

    [Header("Jump Power")]
    [SerializeField] private float _JumpPower;
    [Header("Ground Layer")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] LayerMask Mask;
    private bool grounded;
   
    [Header("Player Camera")]
    [SerializeField] Transform cam;

    //Action Map
    private SlimesCookBook playerInput;
    //Input Variables
    private Vector2 input;
    private Vector2 _RotInput;
    //Controller
    private CharacterController playerChar;

    [Header("Turn Speed")]
    [SerializeField] float turn;
    float _TargetAngle;
    float turnSmoothVel;

    [Header("Animator")]
    [SerializeField] Animator _ControllerAnimator;
    [Header("Blend Speed")]
    [SerializeField] float AnimationBlendSpd;
    private int _XAxis;
    private int _YAxis;
    float _MoveX;
    float _MoveY;

    private void Awake()
    {
        playerInput = new SlimesCookBook();
        playerChar = GetComponent<CharacterController>();
        _ControllerAnimator = GetComponent<Animator>();
        _XAxis = Animator.StringToHash("XAxis");
        _YAxis = Animator.StringToHash("YAxis");
        //_id = Shader.PropertyToID(shaderVariableName);
    }
    
    private void FixedUpdate()
    {
        GroundCheck();
        RaySize();

    }
    private void LateUpdate()
    {
        ApplyGravity();
        Rotation();
        ApplyMove();
        var view = _PlayerCam.WorldToViewportPoint(this.transform.position);
        //_distanceObjectToCamera = _PlayerCam.transform.position - this.transform.position;
        //_Wall.SetVector(shaderVariableName, view);

    }
    void RaySize()
    {
        var dir = _PlayerCam.transform.position - this.transform.position;
        var ray = new Ray(this.transform.position, dir.normalized);
        if(Physics.Raycast(ray, 3000, Mask))
        {
            _Wall.SetFloat("_Height", 0.5f);
        }
        else
        {
            _Wall.SetFloat("_Height", 0f);
        }
    }
    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.Disable();
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        input = context.ReadValue<Vector2>();
        _Direction = new Vector3(input.x, 0, input.y);
    }
    public void OnLook(InputAction.CallbackContext context)
    {
        _RotInput = context.ReadValue<Vector2>().normalized;
        
    }
    void Rotation()
    {
        if (input.sqrMagnitude == 0) return;
        _TargetAngle = Mathf.Atan2(_Direction.x, _Direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, _TargetAngle, ref turnSmoothVel, turn);
        transform.rotation = Quaternion.Euler(0, angle, 0);
               
    }
    void ApplyMove()
    {
       
        movement = Quaternion.Euler(0, _TargetAngle, 0) * Vector3.forward;
        _Speed = input.magnitude * speed;
        _VelocityVec = movement.normalized * _Speed;
              

        _VelocityVec.y += _Direction.y;
        playerChar.Move(_VelocityVec * Time.deltaTime);
        if (!grounded) return;
       
        float _Vel = _VelocityVec.magnitude;
        _MoveY = Mathf.Lerp(_MoveY, _Vel, AnimationBlendSpd * Time.fixedDeltaTime);
        _MoveX = Mathf.Lerp(_MoveX, _RotInput.x, AnimationBlendSpd * Time.fixedDeltaTime);
        _ControllerAnimator.SetFloat(_YAxis, _Vel, AnimationBlendSpd, Time.fixedDeltaTime);
        _ControllerAnimator.SetFloat(_XAxis, _RotInput.x, 0.1f, Time.fixedDeltaTime);
    }
   
    void ApplyGravity()
    {
        if (grounded && _Velocity < 0.0f)
        {
            _Velocity = 0f;
        }
        else
        {
            _Velocity += _Gravity * _GravityMultiplier  * Time.deltaTime;
            
        }
        _Direction.y = _Velocity;
    }
    private bool IsGround() => playerChar.isGrounded;
    void GroundCheck()
    {
        grounded = Physics.CheckSphere(transform.position, 0.5f, groundLayer);
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (!grounded) return;
        _Velocity += _JumpPower;

    }
 
   public void JumpNow()
    {
                
    }
    public void Peaked()
    {
       
    }
  
}
