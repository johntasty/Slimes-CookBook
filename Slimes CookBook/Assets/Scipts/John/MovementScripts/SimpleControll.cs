using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SimpleControll : MonoBehaviour
{
    [Header("Move Speed")] 
    [SerializeField] private float speed;
    private Vector3 _Direction;
    private Vector3 movement;

    [Header("Gravity Power")]
    [SerializeField] float _GravityMultiplier;
    private float _Velocity;
    private float _Gravity = -9.81f;

    [Header("Jump Power")]
    [SerializeField] private float _JumpPower;
    [Header("Ground Layer")]
    [SerializeField] LayerMask groundLayer;
    private bool grounded;

    [Header("Player Camera")]
    [SerializeField] Transform cam;

    //Action Map
    private SlimesCookBook playerInput;
    //Input Variables
    private Vector2 input;
    //Controller
    private CharacterController playerChar;

    [Header("Turn Speed")]
    [SerializeField] float turn;
    float _TargetAngle;

    float turnSmoothVel;

    private void Awake()
    {
        playerInput = new SlimesCookBook();
        playerChar = GetComponent<CharacterController>();
    }
   
    private void FixedUpdate()
    {       
        ApplyGravity();
        Rotation();
        ApplyMove();

    }
    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.Disable();
    }
    private void OnMove(InputValue value)
    {
        input = value.Get<Vector2>();
        _Direction = new Vector3(input.x, 0, input.y);
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
       
        movement = Quaternion.Euler(0, _TargetAngle, 0) * (Vector3.forward * input.magnitude);
        movement.y += _Direction.y;
        playerChar.Move(movement.normalized * speed * Time.deltaTime);

    }
   
    void ApplyGravity()
    {
        if (IsGround() && _Velocity < 0.0f)
        {
            _Velocity = -1.0f;
        }
        else
        {
            _Velocity += _Gravity * _GravityMultiplier  * Time.deltaTime;
            
        }
        _Direction.y = _Velocity;
    }
    private bool IsGround() => playerChar.isGrounded;

    void OnJump()
    {
            
        if (!IsGround()) return;
        _Velocity += _JumpPower;
    }
    
}
