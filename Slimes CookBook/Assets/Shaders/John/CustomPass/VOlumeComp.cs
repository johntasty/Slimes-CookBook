using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/Testing", typeof(UniversalRenderPipeline))]
public sealed class VOlumeComp : VolumeComponent, IPostProcessComponent
{
    [Header("Reaymarch")]
    public IntParameter maxSteps = new IntParameter (64);
    public FloatParameter maxDistance = new FloatParameter (100f);
    public FloatParameter accuracy = new FloatParameter (0.001f);

    public MinFloatParameter intensity = new MinFloatParameter(0f, 0f);
    public bool IsActive() => intensity.value > 0f;
    public bool IsTileCompatible() => true;

    public DepthTextureMode GetCameraFlags()
    {
        return DepthTextureMode.Depth;
    }
}
//public sealed class RayMarchPostProcessRenderer : PostProcessEffectRenderer<RayMarchPostProcess>
//{
//    public override void Render(PostProcessRenderContext context)
//    {
//        Camera _cam = context.camera;

//        var sheet = context.propertySheets.Get(Shader.Find("Custom/TestEffect"));

//        sheet.properties.SetMatrix("_FrustumCornersES", GetFrustumCorners(_cam));
//        sheet.properties.SetMatrix("_CameraToWorld", _cam.cameraToWorldMatrix);
//        sheet.properties.SetVector("_CameraToWorldPosition", _cam.transform.position);
//        sheet.properties.SetInt("_Max_steps", settings.maxSteps);
//        sheet.properties.SetFloat("_Max_Distance", settings.maxDistance);
//        sheet.properties.SetFloat("_DinstanceAccuracy", settings.accuracy);

//        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

//    }

//    private Matrix4x4 GetFrustumCorners(Camera cam)
//    {
//        Matrix4x4 frustrum = Matrix4x4.identity;

//        float fov = Mathf.Tan((cam.fieldOfView * 0.5f) * Mathf.Rad2Deg);

//        Vector3 goUp = Vector3.up * fov;
//        Vector3 goRight = Vector3.right * fov * cam.aspect;

//        Vector3 TL = (-Vector3.forward - goRight + goUp);
//        Vector3 TR = (-Vector3.forward + goRight + goUp);
//        Vector3 BR = (-Vector3.forward + goRight - goUp);
//        Vector3 BL = (-Vector3.forward - goRight - goUp);

//        frustrum.SetRow(0, TL);
//        frustrum.SetRow(1, TR);
//        frustrum.SetRow(2, BR);
//        frustrum.SetRow(3, BL);

//        return frustrum;
//    }
//}
