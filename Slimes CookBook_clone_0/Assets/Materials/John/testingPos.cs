using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//[ExecuteInEditMode]
public class testingPos : MonoBehaviour
{
    private static int PosId = Shader.PropertyToID("_PlayerPos");

    public Material mat;
    public LineRenderer segments;

    public float MoveDuration;
    public float RotationDuration;

    public bool LoopAround = false;

    public float forcePower = 0;
    public float speed;
    public Vector3[] segmentPoints;
    public Vector3 target;
    Vector3 direction;
    public bool moving = false;
    public bool endReached = false;
    public int index = 1;
    
    float distance;
    float distancetraveled;
    //distance measurment
    public float t;

    //coroutines references
    Coroutine moveToPoint = null;
    Coroutine moveUpdate = null;
    Coroutine RotatePoint = null;

    private void Awake()
    {
        RopeIntegration.steUpComplete += StartElevator;
        RopeIntegration.UpdateSegments += UpdatePositions;
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
            
            Accelarate();
        }
        if (Input.GetKeyDown(KeyCode.KeypadEnter)){
            StopCoroutine(moveToPoint);
            moving = false;

        }
    }

    public void Initialize()
    {
        segmentPoints = new Vector3[segments.positionCount];
        segments.GetPositions(segmentPoints);
    }
    void UpdatePositions()
    {
        if (segmentPoints.Length <= 0) return;
        //segments.GetPositions(segmentPoints);
    }
    public void SetUp()
    {  
        target = segmentPoints[index];
        GetDistance();
        direction = (target - transform.position).normalized;
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

        moveToPoint = StartCoroutine(MoveToPoint());
    }
    IEnumerator MoveToPoint()
    {
        moving = true;
       
        float time = 0;
        do {
            time += Time.deltaTime;
            float normalizedTime = time / MoveDuration;

            forcePower = Mathf.Lerp(0, 1, normalizedTime);
            
            yield return null;
        } while (time < MoveDuration);

        float timer = 0;
        float currentSpeed = forcePower;
        do
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / MoveDuration;

            forcePower = Mathf.Lerp(currentSpeed, 0, normalizedTime);           
            yield return null;
        } while (timer < MoveDuration);

        moving = false;       
    }
    IEnumerator MoveUpdate()
    {
        while (true)
        {
            Elevator();
            yield return null;
        }
    }
    IEnumerator RotateToPoint()
    {
        while(t > 1) { yield return null; }
        Quaternion rotateTowards = Quaternion.LookRotation(direction, Vector3.up);
        
        float time = 0;
        while (t < 1)
        {
            if (rotateTowards == Quaternion.identity) { break; }
            time += Time.deltaTime;
            float normalizedTime = time / RotationDuration;
            transform.rotation = Quaternion.Slerp(transform.rotation, rotateTowards, t);

            yield return null;
        }
    }
    void SwitchTarget()
    {
        index++;
        if (index > segmentPoints.Length - 1) { 
            endReached = true;
            forcePower = 0;
            StopCoroutine(moveToPoint);
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
        index = 1;
        endReached = false;
        SetUp();
    }
}
