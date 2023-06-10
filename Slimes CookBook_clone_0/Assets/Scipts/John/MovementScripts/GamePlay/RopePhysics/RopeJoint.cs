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
    public void AddConnection(int pointToAddIndex)
    {
        connection.Add(pointToAddIndex);
    }
    public void SetUpSettings(float spd, float weight, float damp, float springCon, float restLength, RopeConstruct rope)
    {
        previousPosition = transform.position;
        RopeHolder = rope;
        speed = spd;
        mass = weight;
        damping = damp;
        springConstant = springCon;
        restLengths = restLength;       
    }

    public void SimulateJoint(float gravity, float time)
    {
        if (locked) { previousPosition = transform.position; return; }
        Vector3 curr = transform.position;
        velocityVec = transform.position - previousPosition;        
        // calculate new position
        Vector3 newPos = transform.position + velocityVec * damping + Vector3.down * (gravity * mass) * time * time;
      
        transform.position = newPos;
        previousPosition = curr;
    }
    public void ConstrainRope(float time)
    {
        if (anchorPoint) return;

        Vector3 node = RopeHolder.Positions[connection[0]];
        var node1 = RopeHolder.Points[connection[0]];
        float currentDistance = (transform.position - node).magnitude;
        float difference = Mathf.Abs(currentDistance - restLengths);
        Vector3 direction = Vector3.zero;

        if (currentDistance > restLengths)
        {
            direction = (transform.position - node).normalized;
        }
        else if (currentDistance < restLengths)
        {
            direction = (node - transform.position).normalized;
        }
        Vector3 movement = direction * difference;
        // calculate the movement vector
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
