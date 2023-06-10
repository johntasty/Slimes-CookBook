using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MovementAttributes 
{
    [Header("Move Speed")]
    public float speed;
    public float Maxspeed;
    public float TurnSmoothing;
    [Header("Jump Power")]
    public float _JumpPower;

    [Header("Gravity Power")]
    public float _GravityMultiplier;
    public float _Gravity;
    [Header("Collision Layers")]
    public LayerMask groundLayer;
    public LayerMask WallLayer;

    [Header("Player Camera")]
    public Transform cam;

    [Header("Input Map")]
    public SlimesCookBook playerInput;

    [Header("Controller")]
    public CharacterController playerChar;

    [Header("Turn Speed")]
    public float turn;
    [Header("Slime WallCheck Radius")]
    public float radius;
    [Header("Animation")]
    public bool HasAnimation;
    [Header("Animator")]
    public Animator _ControllerAnimator;
    [Header("Blend Speed")]
    public float AnimationBlendSpd;
    public float AnimationBlendTurn;
       
}
