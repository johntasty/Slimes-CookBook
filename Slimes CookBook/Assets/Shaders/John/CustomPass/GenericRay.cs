using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class GenericRay : MonoBehaviour
{
    [SerializeField] Material marcher;
    [SerializeField] float Distance;
    [SerializeField] Transform _Sphere;
    public Transform SunLight;
    [SerializeField]
    private float _RaymarchDrawDistance = 40;
    [SerializeField]
    float radius;
    [SerializeField]
    private Camera _CurrentCamera;
    // Update is called once per frame
    void Update()
    {
        if (marcher == null) return;
      
        marcher.SetMatrix("_FrustumCornersES", GetFrustumCorners(_CurrentCamera));
        marcher.SetMatrix("_CameraToWorld", _CurrentCamera.cameraToWorldMatrix);
        marcher.SetMatrix("_CameraToWorldInverse", _CurrentCamera.cameraToWorldMatrix.inverse);
        marcher.SetFloat("max_Distance", Distance);
        marcher.SetVector("Sphere1", new Vector4(_Sphere.position.x, _Sphere.position.y, _Sphere.position.z, radius));

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
      
}
