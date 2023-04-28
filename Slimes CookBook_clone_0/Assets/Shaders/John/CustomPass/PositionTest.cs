using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class PositionTest : MonoBehaviour
{
    ComputeBuffer bufferShapes;
    [SerializeField]
    ComputeShader _Compute;
    [SerializeField]
    Transform cube;
    [SerializeField]
    Material _Material;
    
    [SerializeField]
    Transform[] _Balls;
    [SerializeField]
    Transform _Target;
    [SerializeField]
    float _Force = 10f;
    [SerializeField]
    float radius = 10f;
    Vector4[] _BallsArray;
    public static Shape[] testing;
    Shape[] shapeData;
   
    static readonly int _BallPosArray = Shader.PropertyToID("_positions");
    static readonly int _BallPos = Shader.PropertyToID("_Position");

    private int mComputeShaderKernelID;
    private int kernelIndex;
    struct ShapeData
    {
        public Vector3 position;
    }
    private void Start()
    {
        //QualitySettings.maxQueuedFrames = 2;
       
        //kernelIndex = _Compute.FindKernel("InvertColors");
       
        //shapeData = new Shape[_Balls.Length];
        //int i = 0;
        //foreach (Transform ball in _Balls)
        //{
        //    Vector3 rand = Random.insideUnitSphere * 2;
        //    ball.position += rand;
        //    shapeData[i].position = new Vector3(ball.position.x, ball.position.y, ball.position.z);
        //    i++;
        //}
        ////testing = shapeData;
        //bufferShapes = new ComputeBuffer(_Balls.Length, sizeof(float) * 3);
        ////mComputeShaderKernelID = _Compute.FindKernel("CSMain");
        //bufferShapes.SetData(shapeData);
        //_Compute.SetBuffer(kernelIndex, "shapes", bufferShapes);
        _BallsArray = new Vector4[40];
        int i = 0;
        foreach (Transform ball in _Balls)
        {
            _BallsArray[i] = new Vector4(ball.position.x, ball.position.y, ball.position.z, radius);
            i++;
        }
        _Material.SetVectorArray(_BallPosArray, _BallsArray);
    }
    //private void OnDestroy()
    //{
    //    if (bufferShapes != null)
    //        bufferShapes.Release();
    //}
    private void Update()
    {
        if (_Material == null) return;
        
        float forceTime = _Force * Time.deltaTime;
        Vector3 PosTarget = _Target.position;
        for (int i = 0; i < _Balls.Length; i++)
        {
            Rigidbody _ball = _Balls[i].GetComponent<Rigidbody>();

            Vector3 direction = Vector3.Normalize(PosTarget - _Balls[i].position);
            Vector3 SeekForce = (direction * forceTime);

            _ball.velocity += SeekForce;
            Vector3 _PosLocal = new Vector3(_ball.position.x, _ball.position.y, _ball.position.z);
            //shapeData[i].position = new Vector3(_PosLocal.x, _PosLocal.y, _PosLocal.z);
            _BallsArray[i] = new Vector4(_PosLocal.x, _PosLocal.y, _PosLocal.z, radius);
        }
        //bufferShapes.SetData(shapeData);
        //testing = shapeData;
         _Material.SetVectorArray(_BallPosArray, _BallsArray);
        //Vector3 pos = cube.position;
        //_Material.SetVector(_BallPos, new Vector3(pos.x, pos.y, pos.z));
    }


}
