using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomPhysics : MonoBehaviour
{
    public float length = 0.1f;
    public float damping = 0.1f;
    public float springConstant = 0.1f;
    public float speed;
    [SerializeField]
    Rigidbody body;

    Vector3 origin;
    [SerializeField]
    public List<Vector3> connections;
    public List<string> names;
    public List<float> restLengths;
    public Vector3 Origin { set => origin = value; }
    public void MoveObject(Vector3 target)
    {
       //each force per connection is made individually
        for (int i = 0; i < connections.Count; i++)
        {
            //target is added to follow the active trasform that has the balls around it
            Vector3 direction = (transform.position - (target + connections[i]));

            float currentLength = (direction).magnitude;

            float restLenghts = currentLength - length;
            //hooks law
            Vector3 force = direction.normalized * (-springConstant * restLenghts) - damping * body.velocity;
            //rigid bodies are used for the collisions, as making my own collision system was way to much work
            body.AddForce(force * speed);
                        
        }              
    }
  
}
