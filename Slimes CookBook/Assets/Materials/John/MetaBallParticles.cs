using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaBallParticles : MonoBehaviour
{
    public Color Color1, Color2;

    [SerializeField]
    List<GameObject> _spheres = new List<GameObject>();
    [SerializeField]
    List<Renderer> renders = new List<Renderer>();
    [SerializeField]
    Renderer _renderer;
    [SerializeField]
    Material _Mat;
    MaterialPropertyBlock _materialPropertyBlock;

    const int MaxParticles = 6;
    int _numParticles;
    
    ParticleSystem.Particle[] _particles = new ParticleSystem.Particle[MaxParticles];
    GameObject[] _Spheres = new GameObject [ MaxParticles];
    readonly Vector4[] _particlesPos = new Vector4[MaxParticles];
    readonly float[] _particlesSize = new float[MaxParticles];

    static readonly int NumParticles = Shader.PropertyToID("_NumParticles");
    static readonly int ParticlesSize = Shader.PropertyToID("_ParticlesSize");
    static readonly int ParticlesPos = Shader.PropertyToID("_ParticlesPos");
    static readonly int PosBall = Shader.PropertyToID("_Pos");
    // Start is called before the first frame update
    private void Start()
    {
        
        //renders = new List<Renderer>();
        int x = 0;
        foreach (GameObject sphere in _spheres)
        {
            _Spheres[x] = sphere;
            //renders.Add(sphere.transform.GetComponent<Renderer>());
            x++;
            if (x >= _spheres.Count) break;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //_particleSystem.GetParticles(_particles, MaxParticles);
        int i = 0;
        foreach (var particle in _Spheres)
        {
            _particlesPos[i] = particle.transform.position;
            _particlesSize[i] = particle.transform.localScale.magnitude;

            ++i;

            if (i >= _numParticles) break;
        }

        _Mat.SetVectorArray(ParticlesPos, _particlesPos);
        _Mat.SetFloatArray(ParticlesSize, _particlesSize);
        _Mat.SetInt(NumParticles, _numParticles);
        _Mat.SetVector(PosBall, transform.position);
        //_renderer.SetPropertyBlock(_materialPropertyBlock);
        //foreach (Renderer rend in renders)
        //{
        //    rend.SetPropertyBlock(_materialPropertyBlock);
        //}
        // Get the current value of the material properties in the renderer.
        //_renderer.GetPropertyBlock(_materialPropertyBlock);
        // Assign our new value.
        //_materialPropertyBlock.Clear();

        // Apply the edited values to the renderer.
        //_renderer.SetPropertyBlock(_materialPropertyBlock);
    }
  

    void OnEnable()
    {
        _numParticles = _spheres.Count;
        //_materialPropertyBlock = new MaterialPropertyBlock();
        
        _Mat = _renderer.sharedMaterial;
    }

    void OnDisable()
    {
        //_materialPropertyBlock.Clear();
        //_materialPropertyBlock = null;
        //_renderer.SetPropertyBlock(null);
        
    }
}
