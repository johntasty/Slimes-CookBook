using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlesCompute : MonoBehaviour
{
    private struct Particle
    {
        public Matrix4x4 mat;
        public Vector3 originPos;
    }
    private const int SIZE_PARTICLE = sizeof(float) * 4 * 4 + sizeof(float) * 3;
    public int particleCount = 1000;
    public Material material;
    public ComputeShader computeShader;
    private int mComputeShaderKernelID;

    ComputeBuffer particleBuffer;
    public Transform target;
    public Transform pusher;
    private const int WARP_SIZE = 256;
    public float testing;
    
    private int mWarpCount;

    // Start is called before the first frame update
    void Start()
    {
        if (particleCount <= 0)
            particleCount = 1;
        mWarpCount = Mathf.CeilToInt((float)particleCount / WARP_SIZE);

        // Initialize the Particle at the start
        Particle[] particleArray = new Particle[particleCount];
        for (int i = 0; i < particleCount; ++i)
        {
            Particle props = new Particle();
            Vector3 rand = Random.insideUnitSphere;
            float x = target.position.x + rand.x;
            float y = target.position.y + rand.y;
            float z = target.position.z + rand.z;
            Vector3 position = new Vector3(x,y,z);
            Quaternion rotation = Quaternion.identity;
            Vector3 scale = Vector3.one;
            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.originPos = position;
            particleArray[i] = props;
        }
      
        // Create the ComputeBuffer holding the Particles
        particleBuffer = new ComputeBuffer(particleCount, SIZE_PARTICLE);
        particleBuffer.SetData(particleArray);

        // Find the id of the kernel
        mComputeShaderKernelID = computeShader.FindKernel("CSMain");

        // Bind the ComputeBuffer to the shader and the compute shader
        computeShader.SetBuffer(mComputeShaderKernelID, "particleBuffers", particleBuffer);
        material.SetBuffer("particleBuffers", particleBuffer);
        computeShader.SetFloat("testDist", testing);

    }
    void OnDestroy()
    {
        if (particleBuffer != null)
            particleBuffer.Release();
    }
    void OnRenderObject()
    {
        material.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points,1,particleCount);
    }
    // Update is called once per frame
    void Update()
    {
      
        // Send datas to the compute shader
        float[] pos = { target.position.x, target.position.y, target.position.z };
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        computeShader.SetFloat("testDist", testing);
        computeShader.SetFloats("target", pos);
        computeShader.SetVector("_PusherPosition", pusher.position);

        // Update the Particles
        computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);
    }
}
