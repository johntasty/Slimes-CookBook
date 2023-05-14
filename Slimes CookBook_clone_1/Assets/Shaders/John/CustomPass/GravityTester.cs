using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityTester : MonoBehaviour
{
    Rigidbody _rigidBody;
       
    Transform target;
    [SerializeField]
    LayerMask metaballs;
    int maxColliders = 10;
    public Transform _Target 
    { 
        get => target;

        set => target = value;
    }
    float distanceMultipliyer;

      
    public float DistanceMultiply
    {
        get => distanceMultipliyer;
        set => distanceMultipliyer = value;
    }
    Vector3 originPoint;
    public Vector3 OriginPoint
    {
        set => originPoint = value;
    }
    private void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }
    public void Attracted(float force)
    {
        Vector3 direction = Vector3.Normalize((_Target.position + originPoint) - transform.position);
        float distance = Vector3.Distance(transform.position, target.position);

        distance = distanceMultipliyer - Mathf.Clamp(distance, 0f, distanceMultipliyer);
      
        Vector3 SeekForce = (direction * (force)) * distance;

        _rigidBody.velocity += SeekForce;
    }
    public void Repel(Vector3 collisionPoint)
    {

        Vector3 direction = Vector3.Normalize(transform.position - collisionPoint);
        Vector3 repelForce = direction * (100);
        _rigidBody.AddForce(repelForce);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Metaball")) return;

        Collider[] points = new Collider[maxColliders];
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, .1f, points, metaballs);
        for (int i = 0; i < numColliders; i++)
        {
            points[i].GetComponent<GravityTester>().Repel(transform.position);
        }
    }

}
