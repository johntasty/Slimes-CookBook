using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCreator : MonoBehaviour
{
   
    // Start is called before the first frame update
    [SerializeField]
    GameObject prefab;
    [SerializeField]
    Transform parent;
    public int rows;
    [SerializeField]
    float radius;
    [SerializeField]
    float range;
    [SerializeField]
    Material _Material;
    Vector4[] _BallsArray;
    [SerializeField]
    Transform[] _Balls;
    static readonly int _BallPosArray = Shader.PropertyToID("_positions");

    List<CustomPhysics> springs = new List<CustomPhysics>();
    void Start()
    {        
        _BallsArray = new Vector4[20];
        //_Balls = new Transform[20];
        //parent = GameObject.FindGameObjectWithTag("BallsParent").transform;
        StartCoroutine(SetUp());
       
    }
    Vector3[] PointsOnSphere(int n)
    {
        
        List<Vector3> upts = new List<Vector3>();
        float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
        float off = 2.0f / 18;
        float x = 0;
        float y = 0;
        float z = 0;
        float r = 0;
        float phi = 0;
      
        for (var k = 0; k < 3; k++)
        {

            y = k * off - 1 + (off / 2);
            r = Mathf.Sqrt(1 - y * y);

            phi = k * inc;
            x = Mathf.Cos(phi) * r;
            z = Mathf.Sin(phi) * r;

            upts.Add(new Vector3(x, Mathf.Cos(phi), z));
        }
        Vector3[] pts = upts.ToArray();
        return pts;
    }
  
    Vector3[] CreateSphericalGrid(int rows, int columns)
    {
        List<Vector3> spherePoints = new List<Vector3>();

        float equatorRadius = 2f;
        float poleRadius = .05f;

        float latStep = Mathf.PI / (rows + 1);
        float lonStep = 2 * Mathf.PI / columns;

      
        for (int i = 1; i <= rows; i++)
        {
            float lat = i * latStep;
           
            float r = Mathf.Lerp(poleRadius, 1f, (float)i / (rows + 1));
            r *= Mathf.Pow(Mathf.Cos(lat), 2);

            for (int j = 0; j < columns; j++)
            {

                float lon = j * lonStep;
                float x = Mathf.Cos(lon) * (1f * ((float)i / rows + 1));
                float y = Mathf.Cos(lat) * equatorRadius;
                float z = Mathf.Sin(lon) * (1f * ((float)i / rows + 1));

                spherePoints.Add(new Vector3(x, y, z));
            }
        }

        Vector3[] pts = spherePoints.ToArray();
        return pts;
    }
    private void LateUpdate()
    {
        MoveObject();
    }
    void MoveObject()
    {
        for (int i = 0; i < springs.Count; i++)
        {
            springs[i].MoveObject(transform.position);
            Vector3 _PosLocal = _Balls[i].position;
            _BallsArray[i] = new Vector4(_PosLocal.x, _PosLocal.y, _PosLocal.z, radius);
        }
        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }
  IEnumerator SetUp()
    {
        Vector3[] points = CreateSphericalGrid(4,5);
        //GameObject ball = null;
        Vector3 pos;
        int i = 0;
        foreach (Vector3 point in points)
        {
            //ball = Instantiate(prefab, parent);
            GameObject ball = _Balls[i].gameObject;
            ball.name = i.ToString();
            pos = point.normalized;
            ball.GetComponent<CustomPhysics>().Origin = pos * range;
            ball.GetComponent<CustomPhysics>().connections.Add(pos * range);
            ball.GetComponent<CustomPhysics>().restLengths.Add(0f);

            springs.Add(ball.GetComponent<CustomPhysics>());
            ball.transform.position = transform.position + (pos * range);
           
            _BallsArray[i] = new Vector4(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z, radius);
            ball.SetActive(true);
            i++;
            yield return null;
        }
        for (int x = 0; x < 5; x++)
        {
            for (int j = 0; j < 3; j++)
            {               
                _Balls[j * (5) + x].GetComponent<CustomPhysics>().connections.Add(transform.InverseTransformPoint(_Balls[(j + 1) * (5) + x].position));
                _Balls[j * (5) + x].GetComponent<CustomPhysics>().restLengths.Add(0.1f);
                _Balls[(j + 1) * (5) + x].GetComponent<CustomPhysics>().connections.Add(transform.InverseTransformPoint(_Balls[j * (5) + x].position));
                _Balls[(j + 1) * (5) + x].GetComponent<CustomPhysics>().restLengths.Add(0.1f);
            }
        }
       
        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }
}