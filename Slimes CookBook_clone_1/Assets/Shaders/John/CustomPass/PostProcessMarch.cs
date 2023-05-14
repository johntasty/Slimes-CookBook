using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;

public class PostProcessMarch : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        RenderTargetIdentifier source;
        RenderTargetIdentifier destinationA;
        RenderTargetIdentifier destinationB;
        RenderTargetIdentifier latestDest;

        readonly int temporaryRTIdA = Shader.PropertyToID("_TempRT");
        readonly int temporaryRTIdB = Shader.PropertyToID("_TempRTB");

        Material _material;
        Camera _cam;
        public CustomRenderPass(Material mat, RenderPassEvent Event)
        {
            // Set the render pass event
            this.renderPassEvent = Event;
            _material = mat;
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.depthBufferBits = 0;
            
            var renderer = renderingData.cameraData.renderer;
            source = renderer.cameraColorTarget;

            // Create a temporary render texture using the descriptor from above.
            cmd.GetTemporaryRT(temporaryRTIdA, descriptor, FilterMode.Bilinear);
            destinationA = new RenderTargetIdentifier(temporaryRTIdA);
            cmd.GetTemporaryRT(temporaryRTIdB, descriptor, FilterMode.Bilinear);
            destinationB = new RenderTargetIdentifier(temporaryRTIdB);


        }
               
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            
            if (_material == null)
            {
                Debug.LogError("Custom Post Processing Materials instance is null");
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get("Custom Post Processing");
            cmd.Clear();
            _cam = renderingData.cameraData.camera;
            var stack = VolumeManager.instance.stack;

            #region Local Methods

            // Swaps render destinations back and forth, so that
            // we can have multiple passes and similar with only a few textures
            void BlitTo(Material mat, int pass = 0)
            {
                var first = latestDest;
                var last = first == destinationA ? destinationB : destinationA;
               
                Blit(cmd, first, last, mat);

                latestDest = last;
            }
           
            #endregion

            // Starts with the camera source
            latestDest = source;

            var customEffect = stack.GetComponent<VOlumeComp>();

            if (customEffect.IsActive())
            {
                // P.s. optimize by caching the property ID somewhere else
                _material.SetMatrix(Shader.PropertyToID("_CameraInverseProjection"), _cam.projectionMatrix.inverse);
                _material.SetMatrix(Shader.PropertyToID("_CameraToWorld"), _cam.cameraToWorldMatrix);
                _material.SetVector(Shader.PropertyToID("_CameraToWorldPosition"), _cam.transform.position);

                _material.SetInt(Shader.PropertyToID("_Max_steps"), customEffect.maxSteps.value);
                _material.SetFloat(Shader.PropertyToID("_Max_Distance"), customEffect.maxDistance.value);
                _material.SetFloat(Shader.PropertyToID("_DinstanceAccuracy"), customEffect.accuracy.value);

                //Blit(cmd, source, destinationA, _material, 0);
                BlitTo(_material);
            }

            Blit(cmd, latestDest, source);
            //cmd.BlitFullscreenTriangle(latestDest, source);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        // Cleanup any allocated resources that were created during the execution of this render pass.
        public override void OnCameraCleanup(CommandBuffer cmd)
        {

            cmd.ReleaseTemporaryRT(temporaryRTIdA);
            cmd.ReleaseTemporaryRT(temporaryRTIdB);
        }

       
    }
    [SerializeField]
    Material _Material;
    [SerializeField]
    RenderPassEvent Event;
    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass(_Material, Event);
               
    }
   
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


