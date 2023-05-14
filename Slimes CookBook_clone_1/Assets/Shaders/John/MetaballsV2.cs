using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballsV2 : MonoBehaviour
{
    Collider coll;
    [SerializeField]
    Vector3 closestPoint;

    static readonly int _Nearest = Shader.PropertyToID("_Positions");
    [SerializeField]
    Material _Mat;
    [SerializeField]
    Vector4[] _Balls;
    [SerializeField]
    Transform[] _ball;
    
    public float radius = 1f; // show penetration into the colliders located inside a sphere of this radius
    public int maxNeighbours = 16; // maximum amount of neighbours visualised
    public float dis;
    public float dis_t;
    private Collider[] neighbours;
    // Start is called before the first frame update
    void Start()
    {
        //coll = GetComponent<Collider>();
        //neighbours = new Collider[maxNeighbours];
        _Balls = new Vector4[_ball.Length];
    }

    // Update is called once per frame
    void Update()
    {        
        SetArray();
    }
    void SetArray()
    {
        for (int i = 0; i < _ball.Length; i++)
        {
            _Balls[i] = _ball[i].position;
        }
        _Mat.SetVectorArray("_Positions", _Balls);
    }
    void _NearestPoint()
    {

        closestPoint = coll.ClosestPoint(transform.position);
       
        //_Mat.SetVector(_Pos, transform.position);

        if (!coll)
            return; // nothing to do without a Collider attached

        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, neighbours);
        
        //if(count <= 0) { dis = 1; }
        for (int i = 0; i < count; ++i)
        {
            var collider = neighbours[i];

            if (collider == coll)
                continue; // skip ourself
           
            Vector3 otherPosition = collider.gameObject.transform.position;
            Quaternion otherRotation = collider.gameObject.transform.rotation;

            Vector3 direction;
            float distance;
            dis = Vector3.Distance(transform.position, otherPosition);
            bool overlapped = Physics.ComputePenetration(
                coll, transform.position, transform.rotation,
                collider, otherPosition, otherRotation,
                out direction, out distance
            );
             
            _Mat.SetFloat(_Nearest, dis - 0.5f);


        }
        }
}
    

