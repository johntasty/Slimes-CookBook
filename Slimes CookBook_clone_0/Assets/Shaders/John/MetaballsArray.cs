using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballsArray : MonoBehaviour
{
    [SerializeField]
    ParticleSystem particles;
    int _numParticles;
    ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[20];

    static readonly int _BallArray = Shader.PropertyToID("_PositionArray");
    [SerializeField]
    Transform[] _Balls = new Transform[12];
    [SerializeField]
    Transform _Ballone;
    public Vector4 _BalloneVect;
    [SerializeField]
    Transform _Balltwo;
    public Vector4 _BalltwoVect;
    public float radius;
    Vector4[] _BallsArray = new Vector4[20];

    [SerializeField]
    Material _Mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
        particles.GetParticles(_particles, 20);
        for (int i = 0; i < _particles.Length; i++)
        {
            Vector3 ball = _particles[i].position;
            _BallsArray[i] = new Vector4(ball.x, ball.y, ball.z, radius);
        }
       
        _Mat.SetVectorArray(_BallArray, _BallsArray);
    }
}
