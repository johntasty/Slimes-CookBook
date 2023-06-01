using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeIntegration : MonoBehaviour
{
    public static Action steUpComplete;
    public static Action UpdateSegments;
    [System.Serializable]
    public struct RopePointSettings
    {
        public float length;
        public float RestLength;
        public float dampin;
        public float springConstant;
        public float _GravityMultiplier;
    }
    [SerializeField]
    RopePointSettings ropeSettings;
    [SerializeField]
    RopeJoint RopeStarter, RopeEnd;
    [SerializeField]
    int RopeSegments;
    [SerializeField]
    float RopeLenght;
    [SerializeField]
    GameObject RopePointPrefab;
    [SerializeField]
    Transform RopePointHolder;

    [SerializeField]
    LineRenderer RopeVisual;
    [SerializeField]
    GameObject RopeVisualPrefab;
    [SerializeField]
    Transform RopeVisualParent;

    float _Gravity = -9.81f;

    [SerializeField]
    List<RopeJoint> points = new List<RopeJoint>();
    [SerializeField]
    List<RopeJoint> elevatorRopes = new List<RopeJoint>();
    public int Iterations = 10;
    public static Vector3[] positions;

    public List<RopeJoint> RopeJoints = new List<RopeJoint>();

    Coroutine updater = null;
    Coroutine RopeConstruct = null;

    public bool activatedElevator = false;
    private void Start()
    {
       
        RopeConstruct = StartCoroutine(RopeConstructor());
        
    }
    private void OnEnable()
    {
        testingPos.ElevetorActivated += ActivateConstruction;
    }
    private void OnDestroy()
    {
        testingPos.ElevetorActivated -= ActivateConstruction;
    }
    void ActivateConstruction()
    {
        activatedElevator = true;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StopCoroutine(updater);
        }
      
    }
    IEnumerator UpdateTick()
    {
        while (true)
        {
            UpdateListPositions();
            
            yield return new WaitForFixedUpdate();
        }
    }
    IEnumerator RopeConstructor()
    {
        points.Add(RopeStarter);
        Vector3 position = points[0].transform.position;
        RopeJoint pointRope = null;
        for (int i = 0; i < RopeSegments; i++)
        {
            position.z += 5;
            GameObject point = Instantiate(RopePointPrefab, position, Quaternion.identity, RopePointHolder);
            pointRope = point.GetComponent<RopeJoint>();
            points.Add(pointRope);
        }
        points.Add(RopeEnd);

        foreach (RopeJoint item in elevatorRopes)
        {
            points.Add(item);
        }
        SetUpJoints();

        while (!activatedElevator) yield return null;
      
        RopeVisualUpdate();
        steUpComplete?.Invoke();
         yield return null;
        
        updater = StartCoroutine(UpdateTick());
    }
    void RopeVisualUpdate()
    {
        if(RopeVisual == null)
        {
            LineRenderer newLine = Instantiate(RopeVisualPrefab, RopeVisualParent).GetComponent<LineRenderer>();
            RopeVisual = newLine;
        }
        RopeVisual.positionCount = points.Count - elevatorRopes.Count;
        RopeVisual.SetPositions(positions);
        UpdateSegments?.Invoke();
    }
    void UpdateListPositions()
    {
        float curentLen = 0;
       
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = points[i].transform.position;
            if (i + 1 > positions.Length - 1) break;
            curentLen += (positions[i + 1] - positions[i]).sqrMagnitude;
        }
        curentLen = Mathf.Sqrt(curentLen);

        RopeVisualUpdate();
        float grav = _Gravity * ropeSettings._GravityMultiplier;
        foreach (RopeJoint point in RopeJoints)
        {
            //Vector3 velocityVec = point.transform.position - point.previousPosition;
            //point.velocityVec = velocityVec;            
            point.SimulateJoint(grav, Time.fixedDeltaTime);
            point.previousPosition = point.transform.position;
            for (int i = 0; i < Iterations; i++)
            {
                point.ConstrainRope(Time.fixedDeltaTime);
            }            
        }
        //DebugPosition();
        if (curentLen > RopeLenght)
        {
            //
        }
    }

    void DebugPosition()
    {
        points[0].previousPosition = points[0].transform.position;
        //points[RopeJoints.Count - 1].previousPosition = points[points.Count - 1].transform.position;

        points[0].VelocityCalculateAnchors();
        //points[points.Count - 1].VelocityCalculateAnchors();

        points[0].ConstrainRope(Time.fixedDeltaTime);
        //points[points.Count - 1].ConstrainRope(Time.fixedDeltaTime);
    }
    void SetUpJoints()
    {
        positions = new Vector3[points.Count];
        foreach (RopeJoint item in points)
        {
            if (item.AnchorPoint) continue;

            item.SetUpSettings(ropeSettings.length, ropeSettings.dampin, 
                ropeSettings.springConstant, ropeSettings.RestLength);
            RopeJoints.Add(item);
        }

        for (int i = 0; i < points.Count - elevatorRopes.Count; i++)
        {
            positions[i] = points[i].transform.position;
            RopeJoint current = points[i];
            if (!current.AnchorPoint)
            {
                current.AddConnection(i - 1);
                if (i == points.Count - 1) break;
                current.AddConnection(i + 1);
            }
        }
        points[0].AddConnection(1);
        points[points.Count - elevatorRopes.Count - 1].AddConnection(points.Count - elevatorRopes.Count - 2);
        int IndexAdd = points.Count - elevatorRopes.Count;
        for (int i = 0; i < elevatorRopes.Count; i++)
        {
            positions[i + IndexAdd] = elevatorRopes[i].transform.position;
            RopeJoint current = elevatorRopes[i];
            if (!current.AnchorPoint)
            {
                current.AddConnection(i + IndexAdd - 1);
                if (i + IndexAdd == points.Count - 1) break;
                current.AddConnection(i + IndexAdd + 1);
            }
        }
       
    }
}
