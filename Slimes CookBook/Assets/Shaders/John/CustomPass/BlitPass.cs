using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public ComputeShader raymarcher;
        public FilterMode filterMode { get; set; }

        public FilteringSettings _filteringSettings { get; set; }

        private BlitSettings settings;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        RenderTargetHandle m_TemporaryColorTexture;
        RenderTargetHandle m_DestinationTexture;
        string m_ProfilerTag;
        RTHandle colorText;
        Camera _cam;
        public CustomRenderPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
        {
            raymarcher = settings.raymacher;
            _filteringSettings = new FilteringSettings(null, settings._layerMask);
            this.renderPassEvent = renderPassEvent;
            this.settings = settings;
            blitMaterial = settings.blitMaterial;
            m_ProfilerTag = tag;

            
            m_TemporaryColorTexture.Init("_TemporaryColorTexture");
            if (settings.dstType == Target.TextureID)
            {
                m_DestinationTexture.Init(settings.dstTextureId);
            }
        }

        public void Setup(ScriptableRenderer renderer)
        {
            if (settings.requireDepthNormals)
            {
                ConfigureInput(ScriptableRenderPassInput.Normal);
                ConfigureInput(ScriptableRenderPassInput.Depth);
            }
               

        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }
       
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            var stack = VolumeManager.instance.stack;
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            _cam = renderingData.cameraData.camera;
            //opaqueDesc.enableRandomWrite = true;
            ConfigureInput(ScriptableRenderPassInput.Color);
            var renderer = renderingData.cameraData.renderer;
            
            if (settings.srcType == Target.CameraColor)
            {
                source = renderer.cameraColorTarget;
            }
            else if (settings.srcType == Target.TextureID)
            {
                source = new RenderTargetIdentifier(settings.srcTextureId);
            }
            else if (settings.srcType == Target.RenderTextureObject)
            {
                source = new RenderTargetIdentifier(settings.srcTextureObject);
            }

            if (settings.dstType == Target.CameraColor)
            {
                destination = renderer.cameraColorTarget;
            }
            else if (settings.dstType == Target.TextureID)
            {
                destination = new RenderTargetIdentifier(settings.dstTextureId);
            }
            else if (settings.dstType == Target.RenderTextureObject)
            {
                destination = new RenderTargetIdentifier(settings.dstTextureObject);
            }

            if (settings.setInverseViewMatrix)
            {
                Shader.SetGlobalMatrix("_InverseView", renderingData.cameraData.camera.cameraToWorldMatrix);
            }

            if (settings.dstType == Target.TextureID)
            {
                if (settings.overrideGraphicsFormat)
                {
                    opaqueDesc.graphicsFormat = settings.graphicsFormat;
                }
                cmd.GetTemporaryRT(m_DestinationTexture.id, opaqueDesc, filterMode);
            }
            var customEffect = stack.GetComponent<VOlumeComp>();
            //Debug.Log($"src = {source},     dst = {destination} ");
            // Can't read and write to same color target, use a TemporaryRT
            if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);


                blitMaterial.SetMatrix(Shader.PropertyToID("_CameraInverseProjection"), _cam.projectionMatrix.inverse);
                blitMaterial.SetMatrix(Shader.PropertyToID("_CameraToWorld"), _cam.cameraToWorldMatrix);
                blitMaterial.SetVector(Shader.PropertyToID("_CameraToWorldPosition"), _cam.transform.position);

                blitMaterial.SetInt(Shader.PropertyToID("_Max_steps"), customEffect.maxSteps.value);
                blitMaterial.SetFloat(Shader.PropertyToID("_Max_Distance"), customEffect.maxDistance.value);
                blitMaterial.SetFloat(Shader.PropertyToID("_DinstanceAccuracy"), customEffect.accuracy.value);

                Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, settings.blitMaterialPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
            }
            else
            {
                Blit(cmd, source, destination, blitMaterial, settings.blitMaterialPassIndex);
            }
           

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private Matrix4x4 FrustumCorners(Camera cam)
        {
            float camFov = cam.fieldOfView;
            float camAspect = cam.aspect;

            Matrix4x4 frustumCorners = Matrix4x4.identity;

            float fovWHalf = camFov * 0.5f;

            float tan_fov = Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            Vector3 toRight = Vector3.right * tan_fov * camAspect;
            Vector3 toTop = Vector3.up * tan_fov;

            Vector3 topLeft = (-Vector3.forward - toRight + toTop);
            Vector3 topRight = (-Vector3.forward + toRight + toTop);
            Vector3 bottomRight = (-Vector3.forward + toRight - toTop);
            Vector3 bottomLeft = (-Vector3.forward - toRight - toTop);

            frustumCorners.SetRow(0, topLeft);
            frustumCorners.SetRow(1, topRight);
            frustumCorners.SetRow(2, bottomRight);
            frustumCorners.SetRow(3, bottomLeft);

            return frustumCorners;
        }
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (settings.dstType == Target.TextureID)
            {
                cmd.ReleaseTemporaryRT(m_DestinationTexture.id);
            }
            if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
            {
                cmd.ReleaseTemporaryRT(m_TemporaryColorTexture.id);
            }
        }
    }
    [System.Serializable]
    public class BlitSettings
    {
        public ComputeShader raymacher;
        public LayerMask _layerMask;
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
      
        public Material blitMaterial = null;
        public int blitMaterialPassIndex = 0;
        public bool setInverseViewMatrix = false;
        public bool requireDepthNormals = false;

        public Target srcType = Target.CameraColor;
        public string srcTextureId = "_CameraColorTexture";
        public RenderTexture srcTextureObject;

        public Target dstType = Target.CameraColor;
        public string dstTextureId = "_BlitPassTexture";
        public RenderTexture dstTextureObject;

        public bool overrideGraphicsFormat = false;
        public UnityEngine.Experimental.Rendering.GraphicsFormat graphicsFormat;
    }
    public enum Target
    {
        CameraColor,
        TextureID,
        RenderTextureObject
    }
    public BlitSettings settings = new BlitSettings();
    CustomRenderPass _BlitPass;

    /// <inheritdoc/>
    public override void Create()
    {
        _BlitPass = new CustomRenderPass(settings.Event, settings, name);
               
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _BlitPass.Setup(renderer);
        renderer.EnqueuePass(_BlitPass);
    }
}


