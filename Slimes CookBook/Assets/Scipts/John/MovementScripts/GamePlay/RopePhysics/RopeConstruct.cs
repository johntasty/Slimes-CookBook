using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeConstruct : MonoBehaviour
{
    //used to signal the network to load the rope
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
    //can either be prebaked or realtime, prebaked is used to create a rope bridge that doesnt update just for a visual
    [SerializeField]
    SimulationTime sim;

    [SerializeField]
    RopePointSettings ropeSettings;

    float _Gravity = 9.81f;
    //points to hold the rope, more can be added
    [SerializeField]
    RopeJoint RopeStarter, RopeEnd;
    public RopeJoint _RopeEnd
    {
        get => RopeEnd;
    }
    //amount of rope segments
    [SerializeField]
    int RopeSegments;
    //stretching of the rope before it pulls the endpoint back or the start point
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
  //sets up the line renderer
    void RopeVisualSetUp()
    {
        RopeVisual.positionCount = points.Count;
        RopeVisual.SetPositions(positions);        
    }
    //updating the line render at each iteration of the simulation
    void RopeVisualUpdate()
    {
        RopeVisual.SetPositions(positions);
    }

    IEnumerator RopeConstructor()
    {
        //anchor start point
        points.Add(RopeStarter);

        Vector3 StartPosition = RopeStarter.transform.position;
        Vector3 EndPosition = RopeEnd.transform.position;
        RopeJoint pointRope = null;
        
        Vector3 direction = (EndPosition - StartPosition).normalized;
        float distance = Vector3.Distance(StartPosition, EndPosition);
        //can be used to create points in a as needed basis instead of predetermined amount
        currentDistance = distance;

        for (int i = 0; i < RopeSegments; i++)
        {
            //spaces out the points
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
            //adds the connections for the ropejoints, unless its start,end
            if (current._EndPoint || current.StartAnchor) continue;

            //current.AddConnection(i - 1);
            current.AddConnection(i + 1);

        }
        //start and end are handled here, avoids array indexing errors
        points[0].AddConnection(1);
        points[points.Count - 1].AddConnection(points.Count - 2);

        RopeVisualSetUp();
        SetUPCompleted = true;
    }

    void UpdateListPositions()
    {
        float grav = _Gravity * ropeSettings._GravityMultiplier;
        //updates all the positions in the array
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = points[i].transform.position;
        }
        //uncomment this to pull end point or anchor point towards the rest of the rope 

        //float dist = (positions[0] - positions[positions.Length - 1]).magnitude;
        //if (dist > MaxStretch) { RopeEnd.AnchorPoint = false; } else
        //{
        //    RopeEnd.AnchorPoint = true;
        //}
        foreach (RopeJoint item in points)
        {
            item.SimulateJoint(grav, Time.fixedDeltaTime);         
        }
        //higher iterations make the rope stiffer and more stable
        for (int i = 0; i < Iterations; i++)
        {
            foreach (RopeJoint item in points)
            {

                item.ConstrainRope(Time.fixedDeltaTime);
            }
        }

    }
   //this is called only when the networked has finished loading the scene, otherwise erros of not found coroutines popup

    public void SimStartup()
    {
        Rope = StartCoroutine(RopeConstructor());
        StartCoroutine(SimCall());
    }
    IEnumerator SimCall()
    {
        //waits for the network to be done loading
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
    //the two types of sims
    //the broadcasr updates visuals and the rope bridge
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
    //coroutine is used for the update to give flexibility in starting and stopping the sim
    //without enabling and disabling the entire script or checking in updates
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
