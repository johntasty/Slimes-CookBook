using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MArchingShader : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        Material material;

        private RenderTargetIdentifier sourceRT;
        private int textureID;
        //set up the render pass, set all properties
        public CustomRenderPass(Material _Material, RenderPassEvent Event)
        {
            // Set the render pass event
            this.renderPassEvent = Event;
            material = _Material;
            textureID = Shader.PropertyToID("_MainTex");

        }
        //initial startup, like Start
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            //creates a new texture
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.colorFormat = RenderTextureFormat.ARGB32;
            //descriptor.enableRandomWrite = true;
            ConfigureInput(ScriptableRenderPassInput.Depth);
            cmd.GetTemporaryRT(textureID, descriptor, FilterMode.Bilinear);
            sourceRT = new RenderTargetIdentifier(textureID);
        }
             
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get("MyCustomRenderFeature");
            //write the cameras view onto a texture
            cmd.Blit(renderingData.cameraData.renderer.cameraColorTarget, sourceRT, material);

            material.SetMatrix(Shader.PropertyToID("_CameraInverseProjection"), renderingData.cameraData.camera.projectionMatrix.inverse);
            material.SetMatrix(Shader.PropertyToID("_CameraWorld"), renderingData.cameraData.camera.cameraToWorldMatrix);
            material.SetVector(Shader.PropertyToID("_CameraToWorldPosition"), renderingData.cameraData.camera.transform.position);
            material.SetFloat(Shader.PropertyToID("_AspectRatio"), renderingData.cameraData.camera.aspect);
            material.SetFloat(Shader.PropertyToID("_OrthoSize"), renderingData.cameraData.camera.orthographicSize);
            //after modifying the camera texture with the mataballs on it, write it back
            cmd.Blit(sourceRT, renderingData.cameraData.renderer.cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();           
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(textureID);
        }
    }

    CustomRenderPass m_ScriptablePass;

    [SerializeField]
    RenderPassEvent Event;
    [SerializeField]
    Material _Material;
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(_Material, Event);
              
    }

   
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


