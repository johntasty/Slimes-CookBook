using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputeParticles : MonoBehaviour
{
	static readonly int _BallArray = Shader.PropertyToID("_positions");
	private struct Shape
	{
		public Vector3 position;		
	}
	private struct Test
	{
		public Vector3 position;
	}
	public Camera cam;

	private const int SIZE_PARTICLE = 24;

	public int particleCount = 1000;
	public float _Range;
	public float NumShapes;
	public float _Size;
	public float _Smooth;
	public float _Width;
	public float _Height;
	[SerializeField]
	Transform _Target;
	[SerializeField]
	Transform _Start;
	public Material material;
	public Material material2;
	public float ShapesAmount;
	public Vector3 emissionSize_;


	public ComputeShader computeShader;	
	private int mComputeShaderKernelID;	
	ComputeBuffer particleBuffer;
	ComputeBuffer particleBufferNormal;
	private const int WARP_SIZE = 256;
	private int mWarpCount;

	Vector3 _Point()
	{
		
		float u = Random.Range(0f,1f);
		float v = Random.Range(0f, 1f);
		float theta = (u * 2.0f * Mathf.PI);
		float phi = Mathf.Acos((2.0f * v - 1.0f));
		float r = Mathf.Sqrt(Random.Range(0f, 1f));
		float sinTheta = Mathf.Sin(theta);
		float cosTheta = Mathf.Cos(theta);
		float sinPhi = Mathf.Sin(phi);
		float cosPhi = Mathf.Cos(phi);
		float x = r * sinPhi * cosTheta;
		float y = r * sinPhi * sinTheta;
		float z = r * cosPhi;
		return new Vector3(x, y, z);

	}
	private void Start()
    {
		//cam = Camera.main;

		//if (particleCount <= 0)
		//	particleCount = 1;
		//mWarpCount = Mathf.CeilToInt((float)particleCount / WARP_SIZE);

		// Initialize the Particle at the start
		Vector4[] particleArray = new Vector4[particleCount];
		
		for (int i = 0; i < particleCount; ++i)
		{
			Vector3 _pos = (_Point()).normalized * _Size;
			particleArray[i].x = _Target.position.x + _pos.x;
			particleArray[i].y = _Target.position.y + _pos.y;
			particleArray[i].z = _Target.position.z + _pos.z;						
		}
		material.SetVectorArray(_BallArray, particleArray);
		//Test[] particleArrayNormal = new Test[particleCount];

		//for (int i = 0; i < particleCount ; ++i)
		//{
		//	Vector3 _pos = (_Point()).normalized * _Size;
		//	particleArrayNormal[i].position.x = 0f;
		//	particleArrayNormal[i].position.y = 0f;
		//	particleArrayNormal[i].position.z = 0f;
		//}
		//// Create the ComputeBuffer holding the Particles
		//particleBuffer = new ComputeBuffer(particleCount, SIZE_PARTICLE);
		//particleBuffer.SetData(particleArray);

		//      particleBufferNormal = new ComputeBuffer(particleCount, SIZE_PARTICLE);
		//      particleBufferNormal.SetData(particleArrayNormal);
		//      // Find the id of the kernel
		//      mComputeShaderKernelID = computeShader.FindKernel("CSMain");

		//// Bind the ComputeBuffer to the shader and the compute shader
		//computeShader.SetBuffer(mComputeShaderKernelID, "shapes", particleBuffer);
		//computeShader.SetBuffer(mComputeShaderKernelID, "_normals", particleBufferNormal);
		//material2.SetBuffer("_normals", particleBufferNormal);
	}

	//void OnDestroy()
	//{
	//	if (particleBuffer != null)
	//		particleBuffer.Release();
	//		particleBufferNormal.Release();
	//}

	// Update is called once per frame
	//void FixedUpdate()
	//{

	//	float[] _target = { _Target.position.x, _Target.position.y, _Target.position.z };
	//	// Send datas to the compute shader
	//	computeShader.SetFloat("deltaTime", Time.deltaTime);	
	//	computeShader.SetFloat("_SphereSmooth", _Smooth);
	//	computeShader.SetFloats("_TargetPosition", _target);
	//	computeShader.SetMatrix("_CameraToWorld", cam.cameraToWorldMatrix);
	//	computeShader.SetMatrix("_CameraInverseProjection", cam.projectionMatrix.inverse);
	//	InitBuf();
	//	int threadGroupsX = Mathf.CeilToInt(_Width / 8.0f);
	//	int threadGroupsY = Mathf.CeilToInt(_Height / 8.0f);
	//	// Update the Particles
	//	computeShader.Dispatch(mComputeShaderKernelID, mWarpCount, 1, 1);
	//}
	//void InitBuf()
 //   {
	//	computeShader.SetFloat("_ShapesNum", ShapesAmount);
	//	computeShader.SetFloat("_ShapesAmount", NumShapes);
	//	computeShader.SetFloat("_Size", _Size);

	//	computeShader.SetFloat("_width", _Width);
	//	computeShader.SetFloat("_height", _Height);

	//}

 //   void OnRenderObject()
	//{
	//	material2.SetPass(0);
	//	Graphics.DrawProceduralNow(MeshTopology.Points, 1, particleCount);
	//}
}
