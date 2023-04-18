using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MetaballsPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
       
        private Material _EffectMaterial;
        private int materialPassIndex;
        
        RenderTargetIdentifier colorBuffer, temporaryBuffer;
        private RenderTargetHandle tempTexture;
        float m_Intensity;

        int temporaryBufferID = Shader.PropertyToID("_MainTex");
        // The profiler tag that will show up in the frame debugger.
        const string ProfilerTag = "ImageEffect";
        public CustomRenderPass(Material material, int passIndex, float Intensity) : base()
        {
            this.materialPassIndex = passIndex;
            this._EffectMaterial = material;
            this.m_Intensity = Intensity;
            tempTexture.Init("_TempTexture");
        }
              
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 32;

            ConfigureInput(ScriptableRenderPassInput.Depth);
            // Grab the color buffer from the renderer camera color target.
            colorBuffer = renderingData.cameraData.renderer.cameraColorTarget;
            cmd.GetTemporaryRT(temporaryBufferID, cameraTextureDesc, FilterMode.Bilinear);
            temporaryBuffer = new RenderTargetIdentifier(temporaryBufferID);
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);
            using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
            {
                // Blit from the color buffer to a temporary buffer and back. This is needed for a two-pass shader.
                //_EffectMaterial.SetFloat("_Intensity", m_Intensity);
                Blit(cmd, colorBuffer, temporaryBuffer, _EffectMaterial, materialPassIndex); // shader pass 0
                Blit(cmd, temporaryBuffer, colorBuffer, _EffectMaterial, materialPassIndex); // shader pass 1
            }

      
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            if (cmd == null) throw new ArgumentNullException("cmd");

            // Since we created a temporary render texture in OnCameraSetup, we need to release the memory here to avoid a leak.
            cmd.ReleaseTemporaryRT(temporaryBufferID);
        }
    }
   
    CustomRenderPass m_ScriptablePass;
    [System.Serializable]
    public class Settings
    {
        public Material material;
        public float Intensity;
        public int materialPassIndex = -1; // -1 means render all passes
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }
    [SerializeField]
    private Settings settings = new Settings();
    public Material Material
    {
        get => settings.material;
    }
    /// <inheritdoc/>
    public override void Create()
    {        
        m_ScriptablePass = new CustomRenderPass(settings.material, settings.materialPassIndex, settings.Intensity);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = settings.renderEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {       
        renderer.EnqueuePass(m_ScriptablePass);
      
    }
}


