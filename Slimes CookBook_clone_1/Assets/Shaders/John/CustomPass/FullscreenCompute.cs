using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenCompute : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        ComputeShader _marcherCompute;
        string _kernelName;
        int _renderTargetId;
        int _renderDestinationId;

        RenderTargetIdentifier _renderTargetIdentifier;
        RenderTargetIdentifier _renderDestinationIdentifier;
        int _renderTextureWidth;
        int _renderTextureHeight;

        Vector3 lightPosition;
        public Shape[] shapeData;
        public int numShapes;
        public float blendStrength;
        public float size;
        List<ComputeBuffer> buffersToDispose;
        public CustomRenderPass(RenderPassEvent renderPassEvent, Settings settings, int targetID, int destId)
        {
            _kernelName = settings.kernelName;
            this.renderPassEvent = settings.Event;
            _marcherCompute = settings._ComputeShader;
            _renderTargetId = targetID;
            shapeData = settings.shapeData;
            numShapes = settings.numShapes;
            blendStrength = settings.blendStrength;
            size = settings.size;
            lightPosition = settings.lightPosition;
            _renderDestinationId = destId;
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            cameraTargetDescriptor.enableRandomWrite = true;
            cmd.GetTemporaryRT(_renderDestinationId, cameraTargetDescriptor);
            _renderTargetIdentifier = new RenderTargetIdentifier(_renderDestinationId);

            _renderTextureWidth = cameraTargetDescriptor.width;
            _renderTextureHeight = cameraTargetDescriptor.height;
            buffersToDispose = new List<ComputeBuffer>();

        }


        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();            
            if (_marcherCompute == null) return;
           
            var mainKernel = _marcherCompute.FindKernel(_kernelName);
            _marcherCompute.GetKernelThreadGroupSizes(mainKernel, out uint xGroupSize, out uint yGroupSize, out _);
           
            cmd.Blit(renderingData.cameraData.targetTexture, _renderTargetIdentifier);
            cmd.SetComputeTextureParam(_marcherCompute, mainKernel, _renderTargetId, renderingData.cameraData.renderer.cameraColorTarget);
            cmd.SetComputeTextureParam(_marcherCompute, mainKernel, _renderDestinationId, _renderTargetIdentifier);
            ComputeBuffer shapeBuffer = new ComputeBuffer(shapeData.Length, sizeof(float) * 3);
            buffersToDispose.Add(shapeBuffer);
            cmd.SetBufferData(shapeBuffer, shapeData);
            cmd.SetComputeBufferParam(_marcherCompute, mainKernel, "shapes", shapeBuffer);
            cmd.SetComputeIntParam(_marcherCompute, "numShapes", shapeData.Length);

            cmd.SetComputeMatrixParam(_marcherCompute, "_CameraToWorld", renderingData.cameraData.camera.cameraToWorldMatrix);
            cmd.SetComputeMatrixParam(_marcherCompute, "_CameraInverseProjection", renderingData.cameraData.camera.projectionMatrix.inverse);
            cmd.SetComputeVectorParam(_marcherCompute,"_Light", lightPosition);
            cmd.SetComputeIntParam(_marcherCompute, "positionLight", 1);

            int threadGroupsX = Mathf.CeilToInt(_renderTextureWidth / xGroupSize);
            int threadGroupsY = Mathf.CeilToInt(_renderTextureHeight / yGroupSize);
            cmd.DispatchCompute(_marcherCompute, mainKernel, threadGroupsX, threadGroupsY, 1);

            cmd.Blit(_renderTargetIdentifier, renderingData.cameraData.renderer.cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_renderTargetId);
            foreach (var buffer in buffersToDispose)
            {
                buffer.Dispose();
            }
        }
    }
  
    [System.Serializable]
    public class Settings
    {
        
        public string kernelName;
        public Material _Material;
        public ComputeShader _ComputeShader;
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public int PassIndex = 0;
        public Shape[] shapeData;

        public int numShapes;
        public float blendStrength;
        public float size;
        public Vector3 lightPosition;
    }
    public Settings settings = new Settings();

    CustomRenderPass m_ScriptablePass;


    public override void Create()
    {
        int renderTargetId = Shader.PropertyToID("Source");
        int renderBufferId = Shader.PropertyToID("Destination");
        m_ScriptablePass = new CustomRenderPass(settings.Event, settings, renderTargetId, renderBufferId);
       
    }
      
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


