using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysics : MonoBehaviour
{
    public float length = 0.1f;
    public float damping = 0.1f;
    public float springConstant = 0.1f;
  
    [SerializeField]
    LayerMask colliders;

    [SerializeField]
    Rigidbody body;

    Vector3 origin;
    [SerializeField]
    public List<Vector3> connections;
    public List<float> restLengths;
    public Vector3 Origin { set => origin = value; }
    public void MoveObject(Vector3 target)
    {
      
        for (int i = 0; i < connections.Count; i++)
        {
            Vector3 direction = (transform.position - (target + connections[i]));
           
            float currentLength = (direction).magnitude;

            float restLenghts = currentLength - restLengths[i];

            Vector3 force = direction.normalized * (-springConstant * restLenghts) - damping * body.velocity;
            body.AddForce(force);
            //Vector3 positionTest = directions.normalized * (-springConstant * restLenghts) - damping * body.velocity;
           
            
        }

        //Vector3 direction = (transform.position - (target + origin));
        //float currentLength = (direction).magnitude;

        //float restLenght = currentLength - length;

        //Vector3 force = direction.normalized * (-springConstant * restLenght) - damping * body.velocity;
        //Debug.DrawLine(transform.position, target + origin, Color.blue);
        //body.AddForce(force);

    }
  
}
