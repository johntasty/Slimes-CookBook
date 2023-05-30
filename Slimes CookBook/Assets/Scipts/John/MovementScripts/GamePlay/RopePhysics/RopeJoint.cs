using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeJoint : MonoBehaviour, IRopePoint
{
    public float speed;
    public float mass;
    public float length;
    public float damping;
    public float springConstant;
    public float restLengths;

    public Vector3 velocityVec;
    public Vector3 previousPosition;
    public List<int> connection = new List<int>();


    [SerializeField]
    bool anchorPoint = false;

    public bool AnchorPoint
    {
        get => anchorPoint;
        set => anchorPoint = value;
    }
    public void AddConnection(int pointToAddIndex)
    {
        connection.Add(pointToAddIndex);
    }
    public void SetUpSettings(float len, float damp, float springCon, float restLength)
    {
        previousPosition = transform.position;

        length = len;
        damping = damp;
        springConstant = springCon;
        restLengths = restLength;       
    }

    public void SimulateJoint(float gravity, float time)
    {
        velocityVec = transform.position - previousPosition;
       
        // calculate new position
        Vector3 newPos = transform.position + velocityVec;
        float gravityWithMass = gravity * mass;
        newPos.y += gravityWithMass * time;
       
        transform.position = newPos;

    }
    public void ConstrainRope(float time)
    {
        for (int i = 0; i < connection.Count; i++)
        {
            Vector3 direction = (transform.position - RopeIntegration.positions[connection[i]]);

            float currentLength = (direction).magnitude;

            float restLenghts = currentLength - restLengths;

            Vector3 force = direction.normalized * (-springConstant * restLenghts) - damping * velocityVec;

            float spd = speed * time;
            transform.position += force * spd;
        }
    }
    public void VelocityCalculateAnchors()
    {
        velocityVec = transform.position - previousPosition;
    }
}
