
using UnityEngine;

public class JellyPhysics : MonoBehaviour
{
    [SerializeField]
    float _BounceSpeed;
    [SerializeField]
    float _Force;
    [SerializeField]
    float _Stiffness;

    private MeshFilter meshFilter;
    private Mesh mesh;

    Vector3[] _MeshVertices;
    Vector3[] _CurrentMeshVertices;
    Vector3[] _VerticeVelocity;


    private void Awake()
    {
        InitMesh();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateVertices();
    }

    private void InitMesh()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;


        _MeshVertices = mesh.vertices;
        _CurrentMeshVertices = new Vector3[_MeshVertices.Length];
        _VerticeVelocity = new Vector3[_MeshVertices.Length];

        for (int i = 0; i < _MeshVertices.Length; i++)
        {
            _CurrentMeshVertices[i] = _MeshVertices[i];
        }
    }

    void UpdateVertices()
    {
        for(int i = 0; i < _CurrentMeshVertices.Length; i++)
        {
            Vector3 _Displacement = _CurrentMeshVertices[i] - _MeshVertices[i];
            _VerticeVelocity[i] -= _Displacement * _BounceSpeed * Time.deltaTime;

            _VerticeVelocity[i] *= 1f - _Stiffness * Time.deltaTime;
            _CurrentMeshVertices[i] += _VerticeVelocity[i] * Time.deltaTime;
        }

        mesh.vertices = _CurrentMeshVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
    void CollisionPressure(Vector3 _Position, float pressure)
    {
        for(int i = 0; i < _CurrentMeshVertices.Length; i++)
        {
            ApplyPressure(i, _Position, pressure);
        }
    }
    void ApplyPressure(int _Index, Vector3 Position, float pressure)
    {
        Vector3 _DistancePoint = _CurrentMeshVertices[_Index] - transform.InverseTransformPoint(Position);

        float PressureForce = pressure / (1f + _DistancePoint.sqrMagnitude);


        float velocity = PressureForce * Time.deltaTime;

        _VerticeVelocity[_Index] += _DistancePoint.normalized * velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Check");
        ContactPoint[] collisionPoints = collision.contacts;

        for(int i = 0; i < collisionPoints.Length; i++)
        {
            Vector3 _Point = collisionPoints[i].point + (collisionPoints[i].point * .1f);
            CollisionPressure(_Point, _Force);
        }
    }
}
