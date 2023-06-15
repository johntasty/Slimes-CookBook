using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeCreator : MonoBehaviour
{
   //physics object to spawn
    [SerializeField]
    GameObject prefab;
    //objects holder to not clutter the scene hierarchy
    [SerializeField]
    Transform parent;
    //rows for the sphere
    public int rows;
    //size
    [SerializeField]
    float radius;
    [SerializeField]
    float range;
    [SerializeField]
    Material _Material;
    Vector4[] _BallsArray;
    //the predetermined amount pf spheres to use, use to be procedural but saw no point to it
    [SerializeField]
    Transform[] _Balls;
    //shader id that will sue the positions to render the raymarched spheres for the metaballs
    static readonly int _BallPosArray = Shader.PropertyToID("_positions");
    //list of physics objects to be updated
    List<CustomPhysics> springs = new List<CustomPhysics>();
    void Start()
    {        
        //size is preset for the amount of physics objects
        _BallsArray = new Vector4[20];      

        StartCoroutine(SetUp());
       
    }
    //creates positions in a sphere for the slims soft body physics
    Vector3[] CreateSphericalGrid(int rows, int columns)
    {
        List<Vector3> spherePoints = new List<Vector3>();
        //expands like a bell shape at the equator
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
    private void FixedUpdate()
    {
        //physics updater
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
        
        Vector3 pos;
        int i = 0;
        foreach (Vector3 point in points)
        {
            //add the spring physics compoments to the spheres
            GameObject ball = _Balls[i].gameObject;
            ball.name = i.ToString();
            pos =  point.normalized * range;
           //origin is for the springs initial position
            ball.GetComponent<CustomPhysics>().Origin = pos;
            ball.GetComponent<CustomPhysics>().connections.Add(pos);
            ball.GetComponent<CustomPhysics>().restLengths.Add(0.1f);

            springs.Add(ball.GetComponent<CustomPhysics>());
            ball.transform.position = transform.position + (pos);
           
            _BallsArray[i] = new Vector4(ball.transform.position.x, ball.transform.position.y, ball.transform.position.z, radius);
            //having them active from the beginning makes loading slower, and introduces drift from the positions, same for gravity
            ball.SetActive(true);
            ball.GetComponent<Rigidbody>().useGravity = true;
            i++;
            yield return null;
        }
        for (int x = 0; x < 5; x++)
        {
            for (int j = 0; j < 3; j++)
            {
                //connections added to each spring
                //as the sphere is made in a spherical fashion, each point is connected to the previous and the next except the last and first
                //forces will be acted on both ends to simulate a soft body physics
                _Balls[j * (5) + x].GetComponent<CustomPhysics>().connections.Add(transform.InverseTransformPoint(_Balls[(j + 1) * (5) + x].position));
                _Balls[j * (5) + x].GetComponent<CustomPhysics>().names.Add(_Balls[(j + 1) * (5) + x].name);
                _Balls[j * (5) + x].GetComponent<CustomPhysics>().restLengths.Add(0.1f);
                _Balls[(j + 1) * (5) + x].GetComponent<CustomPhysics>().connections.Add(transform.InverseTransformPoint(_Balls[j * (5) + x].position));
                _Balls[(j + 1) * (5) + x].GetComponent<CustomPhysics>().names.Add(_Balls[j * (5) + x].name);
                _Balls[(j + 1) * (5) + x].GetComponent<CustomPhysics>().restLengths.Add(0.1f);
            }
        }
       
        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }
}
