using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeJoint : MonoBehaviour, IRopePoint
{
   public  float speed;
   public  float mass;    
   public  float damping;
   public  float springConstant;
   public  float restLengths;

    public Vector3 velocityVec;
    public Vector3 previousPosition;
    public List<int> connection = new List<int>();

    RopeConstruct RopeHolder;

    [SerializeField]
    bool anchorPoint = false;
    public bool AnchorPoint
    {
        get => anchorPoint;
        set => anchorPoint = value;
    }
    [SerializeField]
    bool locked = false;
    public bool Locked
    {
        get => locked;
        set => locked = value;
    }
    [SerializeField]
    bool EndPoint = false;
    public bool _EndPoint
    {
        get => EndPoint;
        set => EndPoint = value;
    }
    [SerializeField]
    bool startAnchor = false;
    public bool StartAnchor
    {
        get => startAnchor;
        set => startAnchor = value;
    }
    //rope connections
    public void AddConnection(int pointToAddIndex)
    {
        connection.Add(pointToAddIndex);
    }
    //rope initializattion
    public void SetUpSettings(float spd, float weight, float damp, float springCon, float restLength, RopeConstruct rope)
    {
        previousPosition = transform.position;
        RopeHolder = rope;
        speed = spd;
        mass = weight;
        damping = damp;
        //spring is no longer used, was tried to get a more stable rope with less loops
        springConstant = springCon;
        restLengths = restLength;       
    }

    public void SimulateJoint(float gravity, float time)
    {
        //if the point is an anchor or creating a rope bridge it wont calculate gravity
        if (locked) { previousPosition = transform.position; return; }
        Vector3 curr = transform.position;
        velocityVec = transform.position - previousPosition;        
        // calculate new position
        //gravity added
        Vector3 newPos = transform.position + velocityVec * damping + Vector3.down * (gravity * mass) * time * time;
      //updating positions to be used for velocity calculation
        transform.position = newPos;
        previousPosition = curr;
    }
    public void ConstrainRope(float time)
    {
        if (anchorPoint) return;
        //all rope points are held in the constructor
        Vector3 node = RopeHolder.Positions[connection[0]];
        var node1 = RopeHolder.Points[connection[0]];
        //getting the distance between each point to add the constraints
        float currentDistance = (transform.position - node).magnitude;
        float difference = Mathf.Abs(currentDistance - restLengths);
        Vector3 direction = Vector3.zero;
        //direction in which to pull the rope point
        if (currentDistance > restLengths)
        {
            direction = (transform.position - node).normalized;
        }
        else if (currentDistance < restLengths)
        {
            direction = (node - transform.position).normalized;
        }
        // calculate the movement vector
        Vector3 movement = direction * difference;
        //moves only points that are not anchors or ends, any point can be made into an anchor 
        if (!startAnchor)
        {
            transform.position -= (movement * speed);            
        }
        if(!node1.AnchorPoint && !EndPoint)
        {
            node1.transform.position += (movement * speed);
        }

    }
  
    public void VelocityCalculateAnchors()
    {
        velocityVec = transform.position - previousPosition;
    }
}
