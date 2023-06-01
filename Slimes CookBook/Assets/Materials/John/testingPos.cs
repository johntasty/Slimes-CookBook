using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class testingPos : MonoBehaviour
{
    public static Action ElevetorActivated;
    private static int PosId = Shader.PropertyToID("_PlayerPos");

    public Material mat;
    [SerializeField]
    LineRenderer segments;
    
    public float MoveDuration;
    public float RotationDuration;

    public bool LoopAround = false;

    public float forcePower = 0;
    public float speed;
    public Vector3[] segmentPoints;
    public Vector3 target;
    public Vector3 direction;
    public Vector3 prevdirection;
    public bool moving = false;
    public bool endReached = false;
    public bool reverse = false;
    public int index = 1;
    
    float distance;
    float distancetraveled;
    //distance measurment
    public float t;
    public bool[] successHits;
    public int successHitsIndex = 0;
    public int hitt = 0;
    public int fail = 0;
   
    Coroutine moveUpdate = null;
    Coroutine RotatePoint = null;

    private void Awake()
    {
        successHits = new bool[5];
        for (int i = 0; i < successHits.Length; i++)
        {
            successHits[i] = false;
        }       
        RopeIntegration.steUpComplete += StartElevator;
        RopeIntegration.UpdateSegments += UpdatePositions;
        ElevatorButtons.MoveElevatorSuccess += SuccessArray;
    }
    private void OnDestroy()
    {
        RopeIntegration.steUpComplete -= StartElevator;
        RopeIntegration.UpdateSegments -= UpdatePositions;
        ElevatorButtons.MoveElevatorSuccess -= SuccessArray;
    }
    private void OnEnable()
    {
        ElevetorActivated?.Invoke();
        StartCoroutine(HitLoop());
        
    }
   
    public void StartElevator()
    {
        Initialize();
        SetUp();
        
    }
    private void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Space))
        {
          
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter)){
           

        }
    }
    IEnumerator HitLoop()
    {
        while (true)
        {
            for (int i = 0; i < successHits.Length; i++)
            {
                if (successHits[i])
                {
                    hitt++;
                    hitt = Mathf.Clamp(hitt, 0, 5);
                    fail = 0;
                    speed += (3 * hitt) * Time.deltaTime;
                    speed = Mathf.Clamp(speed, 0, 15);
                    successHits[i] = false;

                }
                else
                {
                    fail++;
                    fail = Mathf.Clamp(fail, 0, 5);
                    hitt = 0;
                    speed -= fail * Time.deltaTime;
                    speed = Mathf.Clamp(speed, 0, 15);
                }
                yield return new WaitForSeconds(.5f);
            }
            yield return null;
        }        
    }
    void SuccessArray()
    {
        successHits[successHitsIndex] = true;     
        successHitsIndex = (successHitsIndex + 1) % successHits.Length;
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
        RotatePoint = StartCoroutine(RotateToPoint());
    }
    void Elevator()
    {
        float Vel = speed * forcePower;
        distancetraveled += Vel * Time.deltaTime;
        t = distancetraveled / distance;
        Vector3 velocity = direction * Vel * Time.deltaTime;
        transform.position += velocity;
        if (t < 1) return;
        SwitchTarget();
        GetDistance();
    }
    void Accelarate()
    {
        if (moving || endReached) return;
       
      
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

        while (t > 1 || index == 0) {yield return null; }

        do
        {
            Quaternion rotateTowards = Quaternion.LookRotation(direction, Vector3.up);
            //if (rotateTowards == Quaternion.identity) { break; }          
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTowards, t);

            yield return null;
        } while (t < 1) ;
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
        RotatePoint = StartCoroutine(RotateToPoint());
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
        reverse = !reverse;
        SetUp();
    }
}
