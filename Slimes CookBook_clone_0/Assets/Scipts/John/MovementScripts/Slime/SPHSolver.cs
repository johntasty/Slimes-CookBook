using UnityEngine;

public class SPHSolver : MonoBehaviour
{
    [SerializeField]
    GameObject _Prefab;
    [SerializeField]
    Transform _Parent;
    public int number = 10;
    [SerializeField]
    Transform _Target;
    [SerializeField]
    float _Force = 10f;

    [SerializeField]
    Transform[] _Balls;
        
    [SerializeField]
    Material _Mat;
    public float radius;
    static readonly int _BallArray = Shader.PropertyToID("_positions");

    Vector4[] _BallsArray;
    // Start is called before the first frame update
    void Start()
    {
        //_Balls = new Transform[number];
        _BallsArray = new Vector4[number];
        int i = 0;
        foreach(Transform ball in _Balls)
        {
            _BallsArray[i] = new Vector4(ball.position.x, ball.position.y, ball.position.z, radius);
            i++;
        }
        //for (int i = 0; i < number; i++)
        //{
        //    float ran = Random.Range(0.5f, 1.5f);
        //    Vector3 sp = Random.insideUnitSphere;
        //    Transform _ball = Instantiate(_Prefab, _Target.position + sp, Quaternion.identity, _Parent).transform;
        //    _Balls[i] = _ball;
        //    _BallsArray[i] = new Vector4(_ball.position.x, _ball.position.y, _ball.position.z, radius);
        //}
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float forceTime = _Force * Time.deltaTime;
        Vector3 PosTarget = _Target.position;
        for (int i = 0; i < _Balls.Length; i++)
        {
            Rigidbody _ball = _Balls[i].GetComponent<Rigidbody>();

            Vector3 direction = Vector3.Normalize(PosTarget - _Balls[i].position);
            Vector3 SeekForce = (direction * forceTime);

            _ball.velocity += SeekForce;
            Vector3 _PosLocal = new Vector3(_ball.position.x, _ball.position.y, _ball.position.z);
            //_PosLocal = transform.InverseTransformPoint(_PosLocal);
            _BallsArray[i] = new Vector4(_PosLocal.x, _PosLocal.y, _PosLocal.z, radius);
        }
        _Mat.SetVectorArray(_BallArray, _BallsArray);
        
    }
}
