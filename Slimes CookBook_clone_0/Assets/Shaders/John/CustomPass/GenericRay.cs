using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class GenericRay : MonoBehaviour
{
    [SerializeField] Material marcher;
    public Transform SunLight;
    [SerializeField]
    private float _RaymarchDrawDistance = 40;

    [SerializeField]
    private Camera _CurrentCamera;
    // Update is called once per frame
    void Update()
    {
        if (marcher == null) return;
       
        marcher.SetMatrix("_FrustumCornersES", GetFrustumCorners(_CurrentCamera));
        marcher.SetMatrix("_CameraInvViewMatrix", _CurrentCamera.cameraToWorldMatrix);
        marcher.SetVector("_CamWorldPosition", _CurrentCamera.transform.position);
       
        //marcher.SetMatrix("_CameraInvViewMatrix", _CurrentCamera.cameraToWorldMatrix);
        //marcher.SetVector("_CameraWS", _CurrentCamera.transform.position);
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

    static void CustomGraphicsBlit()
    {
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
    }

}
