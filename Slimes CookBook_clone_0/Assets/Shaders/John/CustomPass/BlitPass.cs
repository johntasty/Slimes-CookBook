using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blitMaterial = null;
        public FilterMode filterMode { get; set; }
        public Vector4 sphere { get; set; }

        private BlitSettings settings;

        private RenderTargetIdentifier source { get; set; }
        private RenderTargetIdentifier destination { get; set; }

        RenderTargetHandle m_TemporaryColorTexture;
        RenderTargetHandle m_DestinationTexture;
        string m_ProfilerTag;

        public CustomRenderPass(RenderPassEvent renderPassEvent, BlitSettings settings, string tag)
        {
            this.sphere = settings.sphere;
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
                ConfigureInput(ScriptableRenderPassInput.Normal);

        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }
       
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

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

            //Debug.Log($"src = {source},     dst = {destination} ");
            // Can't read and write to same color target, use a TemporaryRT
            if (source == destination || (settings.srcType == settings.dstType && settings.srcType == Target.CameraColor))
            {
                cmd.GetTemporaryRT(m_TemporaryColorTexture.id, opaqueDesc, filterMode);
                blitMaterial.SetMatrix("_FrustumCornersES", GetFrustumCorners(renderingData.cameraData.camera));
                blitMaterial.SetMatrix("_CameraToWorld", renderingData.cameraData.camera.cameraToWorldMatrix);
                blitMaterial.SetFloat("max_Distance", 100f);
                blitMaterial.SetVector("Sphere1", sphere);
                Blit(cmd, source, m_TemporaryColorTexture.Identifier(), blitMaterial, settings.blitMaterialPassIndex);
                Blit(cmd, m_TemporaryColorTexture.Identifier(), destination);
            }
            else
            {
                Blit(cmd, source, destination, blitMaterial, settings.blitMaterialPassIndex);
            }
            GL.PushMatrix();
            GL.LoadOrtho(); // Note: z value of vertices don't make a difference because we are using ortho projection

            GL.Begin(GL.QUADS);

            // Here, GL.MultitexCoord2(0, x, y) assigns the value (x, y) to the TEXCOORD0 slot in the shader.
            // GL.Vertex3(x,y,z) queues up a vertex at position (x, y, z) to be drawn.  Note that we are storing
            // our own custom frustum information in the z coordinate.
            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

            GL.End();
            GL.PopMatrix();

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private Matrix4x4 GetFrustumCorners(Camera cam)
        {
            Matrix4x4 frustrum = Matrix4x4.identity;

            float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Rad2Deg);

            Vector3 goUp = Vector3.up * fov;
            Vector3 goRight = Vector3.right * fov * cam.aspect;

            Vector3 TL = (-Vector3.forward - goRight + goUp);
            Vector3 TR = (-Vector3.forward + goRight + goUp);
            Vector3 BR = (-Vector3.forward + goRight - goUp);
            Vector3 BL = (-Vector3.forward - goRight - goUp);

            frustrum.SetRow(0, TL);
            frustrum.SetRow(1, TR);
            frustrum.SetRow(2, BR);
            frustrum.SetRow(3, BL);

            return frustrum;
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
        public RenderPassEvent Event = RenderPassEvent.AfterRenderingOpaques;
        public Vector4 sphere;
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


