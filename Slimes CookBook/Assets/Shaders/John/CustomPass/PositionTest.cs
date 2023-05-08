using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PositionTest : MonoBehaviour
{
    [SerializeField]
    LayerMask ground;
    [SerializeField]
    LayerMask metaballs;
    bool grounded = false;
    [SerializeField]
    float distance;
    //velocity Jumping
    private float _Velocity;
    [SerializeField]
    private float _Gravity;
    [SerializeField]
    private float MultiplyerGravity;

    CharacterController controller;
    //movement velocity
    Vector3 _VelocityVec;

    [SerializeField]
    Transform cube;
    [SerializeField]
    Material _Material;
    
    [SerializeField]
    Transform[] _Balls;
    List<GravityTester> compoments = new List<GravityTester>();
    [SerializeField]
    Transform _Target;
    [SerializeField]
    float _Force = 10f;
    [SerializeField]
    float _Speed = 2f;
    [SerializeField]
    float radius = 10f;
    [SerializeField]
    float range = .2f;
    Vector4[] _BallsArray;
    Collider[] balls;
    static readonly int _BallPosArray = Shader.PropertyToID("_positions");
     
   
    private void Start()
    {
        
        controller = GetComponent<CharacterController>();
        CompomentList();
        _BallsArray = new Vector4[20];
        int i = 0;
        Vector3[] points = PointsOnSphere(_Balls.Length);
        foreach (Transform ball in _Balls)
        {
            Vector3 pos = points[i].normalized;
            ball.transform.position = _Target.position + (pos * range);
            compoments[i].OriginPoint = pos * range;
            _BallsArray[i] = new Vector4(ball.position.x, ball.position.y, ball.position.z, radius);
            i++;
        }
        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }

    private void FixedUpdate()
    {
        GroundChecker();
        Gravity();
        MovementVector();
        
    }
    private void Update()
    {
        controller.Move(_VelocityVec * Time.deltaTime);
        AttractSpheres();
    }
    Vector3[] PointsOnSphere(int n)
    {
        List<Vector3> upts = new List<Vector3>();
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / n;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;

        for (var k = 0; k < n; k++)
        {
            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);
            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, y, z));
        }
        Vector3[] pts = upts.ToArray();
        return pts;
    }
    void CompomentList()
    {
        foreach (Transform ball in _Balls)
        {
            GravityTester balls = ball.GetComponent<GravityTester>();
            balls._Target = this.transform;
            balls.DistanceMultiply = distance;
            compoments.Add(balls);
        }
    }
    void AttractSpheres()
    {
        if (_Material == null) return;
        float forceTime = _Force * Time.deltaTime;       
        for (int i = 0; i < _Balls.Length; i++)
        {
            compoments[i].DistanceMultiply = distance;
            compoments[i].Attracted(forceTime);
            Vector3 _PosLocal = _Balls[i].position;
            _BallsArray[i] = new Vector4(_PosLocal.x, _PosLocal.y, _PosLocal.z, radius);
        }

        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }
    public void MovementVector()
    {       
        _VelocityVec.y = _Velocity;
    }
    public void Gravity()
    {
        if (grounded && _Velocity < 0.0f)
        {
            _Velocity = -1f;
            return;
        }
        _Velocity += _Gravity * MultiplyerGravity * Time.deltaTime;
    }
    public void GroundChecker()
    {
        grounded = Physics.CheckSphere(transform.position, .5f, ground);
    }
    void Overlaps()
    {
        balls = Physics.OverlapSphere(transform.position, .5f, metaballs);
    }
}
