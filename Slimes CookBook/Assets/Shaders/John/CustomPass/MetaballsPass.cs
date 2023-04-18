using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MetaballsPass : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
       
        private Material _EffectMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;

        // The profiler tag that will show up in the frame debugger.
        const string ProfilerTag = "ImageEffect";
        public CustomRenderPass(Material material) : base()
        {
            this._EffectMaterial = material;
            tempTexture.Init("_TempTexture");
        }

        public void SetSource(RenderTargetIdentifier source)
        {
            this.source = source;
        }
        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in a performant manner.
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {

        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler(ProfilerTag)))
            {
                RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
                cameraTextureDesc.depthBufferBits = 0;
                cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Bilinear);

                Blit(cmd, source, tempTexture.Identifier(), _EffectMaterial, 0);
                Blit(cmd, tempTexture.Identifier(), source);
            }
           
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
    [SerializeField]
    private Shader _EffectShader;

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        var material = new Material(_EffectShader);
        m_ScriptablePass = new CustomRenderPass(material);

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


