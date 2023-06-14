using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class testingPos : NetworkBehaviour
{
    [SerializeField]
    GameObject ReturnSwitch;
    [SerializeField]
    LineRenderer segments;
    [SerializeField]
    Transform propeler;
    [SerializeField]
    Transform Rotor;
    bool propellerRotating = false;
    [SerializeField]
    float propellerSpeed = 1f;
    public float MoveDuration;
    public float RotationDuration;

    public bool LoopAround = false;

    public float forcePower = 0;
    public float speed;
    public Vector3[] segmentPoints;
    public Vector3 target;
    public Vector3 direction;
    public Vector3 prevdirection;
    Vector3 _Velocity;
    public float vSpeed;
    public Vector3 Velocity
    {
        get => _Velocity;
    }
    public bool moving = false;
    public bool endReached = false;

    [SyncVar]
    public bool reverse = false;
    public int index = 1;
    
    float distance;
    float distancetraveled;
    //distance measurment
    public float t;
 
    [SyncVar]
    public int successHitsIndex = 0;

    Coroutine moveUpdate = null;
    Coroutine RotatePoint = null;
    [SerializeField]
    RopeIntegration ropeMaker;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(AccelarateElevator());
        }
    }
    private void Awake()
    {
        RopeConstruct.setupComplete += StartElevator;       
        ElevatorButtons.MoveElevatorSuccess += SuccessArray;
        ElevatorButtons.ReturnJourney += Return;
    }
    private void OnDestroy()
    {       
        RopeConstruct.setupComplete -= StartElevator;     
        ElevatorButtons.MoveElevatorSuccess -= SuccessArray;
        ElevatorButtons.ReturnJourney -= Return;
    }
    private void OnEnable()
    {
       
    }
   
    public void StartElevator()
    {
        Initialize();
        SetUp();
        
    }
  IEnumerator PropellerStart()
    {
        propellerRotating = true;
        
        float time = 0;
        float duration = 1f;
        
        do
        {
            time += Time.deltaTime;
            float normalTime = time / duration;
            float angle = (360f * normalTime);
            Quaternion RotateAngle = Quaternion.AngleAxis(angle* propellerSpeed, Vector3.forward);
            propeler.localRotation = Quaternion.Slerp(propeler.localRotation, RotateAngle, normalTime);
            yield return null;

        } while (time < duration);
        if(successHitsIndex >= 4)
        {
            StartCoroutine(AccelarateElevator());
        }
        if (reverse)
        {
            StartCoroutine(AccelarateElevator());
        }
        propellerRotating = false;
        
    }
    IEnumerator AccelarateElevator()
    {
        speed = .1f;
        StartCoroutine(HitLoop());
        float time = 0;
        float duration = 5;
        float normalTime = 0;
        do
        {
            time += Time.deltaTime;
            normalTime = time / duration;
            speed = Mathf.Lerp(0, 15, normalTime);
            yield return new WaitForFixedUpdate();
        } while (time < duration);
    }
    IEnumerator HitLoop()
    {
        propellerRotating = true;
        int anglez = 0;
      
        while (speed > 0)
        {
            anglez = (anglez + 1) % 360;          
            Quaternion RotateAngle = Quaternion.AngleAxis(anglez * propellerSpeed, Vector3.right);
            Quaternion RotateAngleRotor = Quaternion.AngleAxis(anglez * speed, Vector3.forward);
            propeler.localRotation = RotateAngle;

            Rotor.localRotation = RotateAngleRotor;
            yield return new WaitForFixedUpdate();
        }        
    }
    void SuccessArray()
    {
        if (propellerRotating) return;
        CmdSuccessHit();                     
    }
    void Return()
    {
        if (propellerRotating) return;
        CmdReturnJourney();          
    }
    [Command(requiresAuthority = false)]
    void CmdSuccessHit()
    {
        if (successHitsIndex >= 4 || reverse) return;        
        successHitsIndex++;
        RpcStartupEngine();
    }
    [ClientRpc]
    void RpcStartupEngine()
    {
        StartCoroutine(PropellerStart());
    }
    [Command(requiresAuthority = false)]
    void CmdReturnJourney()
    {
        successHitsIndex = 0;
        RpcStartupReturn();
    }
    [ClientRpc]
    void RpcStartupReturn()
    {
        StartCoroutine(PropellerStart());
    }
    public void Initialize()
    {
        segmentPoints = new Vector3[segments.positionCount];
        segments.GetPositions(segmentPoints);
    }
    void UpdatePositions()
    {
        if (segmentPoints.Length <= 0) return;
        if (!reverse)
        {

            segments.GetPositions(segmentPoints);
            target = segmentPoints[index];
            
            direction = (target - transform.position).normalized;
            if(direction != prevdirection)
            {
                GetDistance();
                prevdirection = direction;
            }
            return;
        }
       
    }
    public void SetUp()
    {  
        target = segmentPoints[index];
        GetDistance();
        direction = (target - transform.position).normalized;
        
        prevdirection = direction;
        moveUpdate = StartCoroutine(MoveUpdate());
        StartCoroutine(RotateToFirstPoint());
       
    }
    void Elevator()
    {
        float Vel = speed * forcePower;
        distancetraveled += Vel * Time.deltaTime;
        t = distancetraveled / distance;        
         Vector3 dir = direction * Vel * Time.deltaTime;
        _Velocity = direction * Vel;
        vSpeed = Vel;
        transform.position += dir;
        //SlopeRope(t);
        if (t < 1) return;
        SwitchTarget();
        GetDistance();
    }
   

    IEnumerator MoveUpdate()
    {
        while (true)
        {
            Elevator();
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator RotateToPoint()
    {

        //while (t > 1 || index == 0) {yield return null; }

        while (t < 1)
        {
            Quaternion rotateTowards = Quaternion.LookRotation(direction, Vector3.up);
               
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTowards, t);

            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator RotateToFirstPoint()
    {

        //while (t > 1 || index == 0) {yield return null; }
        float time = 0;
        float normaltime = 0;
        float duration = 10f;
        do
        {
            time += Time.deltaTime;
            normaltime = time / duration;
            Quaternion rotateTowards = Quaternion.LookRotation(direction, Vector3.up);

            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTowards, normaltime);

            yield return null;
        } while (time < duration);
    }
    void SwitchTarget()
    {
        index++;
        if (index > segmentPoints.Length - 1) { 
            endReached = true;
            speed = 0;           
            StopCoroutine(moveUpdate);
            if (LoopAround)
            {
                LoopBack();
                return;
            }
            return; }       
        target = segmentPoints[index];
        direction = (target - transform.position).normalized;
        if(index != 0) {
            t = 0;
            RotatePoint = StartCoroutine(RotateToPoint());
            return;
        }
        StartCoroutine(RotateToFirstPoint());
    }
    void GetDistance()
    {        
        distancetraveled = 0;
        distance = Vector3.Distance(transform.position, target);
    }
    void LoopBack()
    {
        Vector3[] reverseOrder = new Vector3[segmentPoints.Length];
        int loop = segmentPoints.Length - 1;
        for (int i = loop; i >= 0; i--)
        {
            Vector3 point = segmentPoints[i];
            reverseOrder[loop - i] = point;
        }
        segmentPoints = reverseOrder;
        index = 0;
        endReached = false;
        CmdReverseOrder();
        ActivateReturnSwitch();
        SetUp();
    }
    [Server]
    void CmdReverseOrder()
    {
        reverse = !reverse;
    }
    void ActivateReturnSwitch()
    {
        bool active = !ReturnSwitch.activeInHierarchy;
        ReturnSwitch.SetActive(active);
    }
}
