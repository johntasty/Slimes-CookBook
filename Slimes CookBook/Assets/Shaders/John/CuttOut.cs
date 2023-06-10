using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class CuttOut : MonoBehaviour
{
    [SerializeField] Material WallMat;

    public GameObject TargetCutLocation;
    public GameObject FixedForward;
    public Vector2 _targetCutOffset = new Vector2(.5f, 0f);
    public Vector2 scaleWindow = new Vector2(1, 1);
    public Vector3 posss;
    private static int PosId = Shader.PropertyToID("_CutoutMask1MapWorldPos");
    //private static int mapSt = Shader.PropertyToID("_CutoutMask1Map_ST");
    private static int cutBool = Shader.PropertyToID("cutShape");
    private MaterialPropertyBlock propertyBlock;
    public bool cut = false;
    private void Update()
    {
        Cut();
    }
    void Cut()
    {
        //cut = true;
        propertyBlock = new MaterialPropertyBlock();
        //var heading = posss - TargetCutLocation.transform.position;
        //var yDist = heading.y;
        //heading.y = 0f;
        //var distance = heading.magnitude;

        //var dot = Vector3.Dot(heading, transform.right);
        //var signedDist = distance * Mathf.Sign(dot);

        //var flip = 1f;
        //var angle = Vector3.Angle(transform.forward, FixedForward.transform.forward);
        //if (Mathf.Abs(angle) >= 180) flip = -1;

        //var scale = new Vector2(scaleWindow.x / 2, scaleWindow.y / 2);
       
        float scale = GetComponent<MeshFilter>().mesh.bounds.center.y;
        var wallMat = TargetCutLocation.GetComponent<Renderer>();
        //propertyBlock.SetVector(mapSt,new Vector4(scale.x, scale.y, _targetCutOffset.x, _targetCutOffset.y));
        propertyBlock.SetVector(PosId, new Vector4(transform.position.x, transform.position.y + scale - .5f, transform.position.z, 0));
        propertyBlock.SetFloat(cutBool, 1f);
        wallMat.SetPropertyBlock(propertyBlock);
    }
}
