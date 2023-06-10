using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeConstruct : MonoBehaviour
{
    public static Action setupComplete;
    [SerializeField]
    bool BroadCastEvent;
    [System.Serializable]
    public struct RopePointSettings
    {
        public float speed;
        public float mass;
        public float RestLength;
        public float dampin;
        public float springConstant;
        public float _GravityMultiplier;
    }
    enum SimulationTime
    {
        SetTimeSim,
        ConstantSim,
    }

    [SerializeField]
    SimulationTime sim;

    [SerializeField]
    RopePointSettings ropeSettings;

    float _Gravity = 9.81f;

    [SerializeField]
    RopeJoint RopeStarter, RopeEnd;
    public RopeJoint _RopeEnd
    {
        get => RopeEnd;
    }
    [SerializeField]
    int RopeSegments;
    [SerializeField]
    float MaxStretch;
    float currentDistance;
    [SerializeField]
    int SimulationsItteration;
    [SerializeField]
    LineRenderer RopeVisual;
   
    [SerializeField]
    GameObject RopePointPrefab;
    [SerializeField]
    Transform RopePointHolder;


    [SerializeField]
    List<RopeJoint> points = new List<RopeJoint>();
    public List<RopeJoint> Points
    {
        get => points;        
    }

    Vector3[] positions;
    public Vector3[] Positions
    {
        get => positions;
        set => positions = value;
    }
    [SerializeField]
    int Iterations = 10;

    Coroutine Rope = null;

    Coroutine SimulationConstant = null;
    public Coroutine _SimulationConstant
    {
        get => SimulationConstant;
    }

    Coroutine SimulationSet = null;
    public Coroutine _SimulationSet
    {
        get => SimulationSet;
    }

    bool SetUPCompleted = false;
  
    void RopeVisualSetUp()
    {
        RopeVisual.positionCount = points.Count;
        RopeVisual.SetPositions(positions);        
    }
    void RopeVisualUpdate()
    {
        RopeVisual.SetPositions(positions);
    }
    IEnumerator RopeConstructor()
    {
        points.Add(RopeStarter);
        Vector3 StartPosition = RopeStarter.transform.position;
        Vector3 EndPosition = RopeEnd.transform.position;
        RopeJoint pointRope = null;
        Vector3 direction = (EndPosition - StartPosition).normalized;
        float distance = Vector3.Distance(StartPosition, EndPosition);
        currentDistance = distance;
        for (int i = 0; i < RopeSegments; i++)
        {
            Vector3 position = direction * (distance / RopeSegments) * (i + 1);
            GameObject point = Instantiate(RopePointPrefab, position, Quaternion.identity, RopePointHolder);
            pointRope = point.GetComponent<RopeJoint>();
            pointRope.previousPosition = point.transform.position;
            points.Add(pointRope);
        }
        points.Add(RopeEnd);
               
        SetUpJoints();
        
        yield return null;

    }

    void SetUpJoints()
    {
        RopeConstruct rope = this.transform.GetComponent<RopeConstruct>();
        positions = new Vector3[points.Count];
        foreach (RopeJoint item in points)
        {
            item.SetUpSettings(ropeSettings.speed, ropeSettings.mass, ropeSettings.dampin,
                ropeSettings.springConstant, ropeSettings.RestLength,
                rope);           
        }

        for (int i = 0; i < points.Count; i++)
        {
            positions[i] = points[i].transform.position;
            RopeJoint current = points[i];
            if (current._EndPoint || current.StartAnchor) continue;

            //current.AddConnection(i - 1);
            current.AddConnection(i + 1);

        }
        points[0].AddConnection(1);
        points[points.Count - 1].AddConnection(points.Count - 2);

        RopeVisualSetUp();
        SetUPCompleted = true;
    }

    void UpdateListPositions()
    {
        float grav = _Gravity * ropeSettings._GravityMultiplier;
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = points[i].transform.position;
        }
        //float dist = (positions[0] - positions[positions.Length - 1]).magnitude;
        //if (dist > MaxStretch) { RopeEnd.AnchorPoint = false; } else
        //{
        //    RopeEnd.AnchorPoint = true;
        //}
        foreach (RopeJoint item in points)
        {
            item.SimulateJoint(grav, Time.fixedDeltaTime);         
        }
        for (int i = 0; i < Iterations; i++)
        {
            foreach (RopeJoint item in points)
            {

                item.ConstrainRope(Time.fixedDeltaTime);
            }
        }

    }
   
    public void SimStartup()
    {
        Rope = StartCoroutine(RopeConstructor());
        StartCoroutine(SimCall());
    }
    IEnumerator SimCall()
    {
        while (!SetUPCompleted) yield return null;
        SimmulationStart();
    }
    public void SimmulationStart()
    {
        switch (sim)
        {
            case SimulationTime.ConstantSim:
                SimulationConstant = StartCoroutine(UpdateTick());
                break;
            case SimulationTime.SetTimeSim:
                SimulationSet = StartCoroutine(SimulateForSet());
                break;
            
        }
    }
    IEnumerator SimulateForSet()
    {
        for (int i = 0; i < SimulationsItteration; i++)
        {           
            UpdateListPositions();
            yield return new WaitForFixedUpdate();
        }
        RopeVisualUpdate();
        if (BroadCastEvent)
        {
            setupComplete?.Invoke();
        }
    }
    IEnumerator UpdateTick()
    {
        while (true)
        {
            UpdateListPositions();
            RopeVisualUpdate();
            yield return new WaitForFixedUpdate();
        }
    }
}
