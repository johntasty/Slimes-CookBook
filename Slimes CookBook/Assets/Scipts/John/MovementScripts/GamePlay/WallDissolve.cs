using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDissolve : MonoBehaviour
{
    private static int PosId = Shader.PropertyToID("_PlayerPos");

    [SerializeField]
    Material WallDissolveMat;
    private Camera MainCam;
    [SerializeField]
    LayerMask Masks;
    public float dissolveAmount;
    [SerializeField]
    Transform LookAt;

    public List<Renderer> rendererCache = new List<Renderer>();

    private MaterialPropertyBlock propertyBlock;
    public void WallSetup()
    {
        MainCam = transform.GetComponentInChildren<Camera>();
        if (propertyBlock == null)
            propertyBlock = new MaterialPropertyBlock();
    }
    public void WallDissolusion()
    {
        Vector3 dir = MainCam.transform.position - LookAt.position;
        Ray ray = new Ray(transform.position, dir.normalized);
        Vector3 view = MainCam.WorldToViewportPoint(LookAt.position);
        Debug.DrawRay(transform.position, dir.normalized * 50, Color.green);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 50, Masks))
        {
            if (!rendererCache.Contains(hit.transform.GetComponent<Renderer>()))
            {
                rendererCache.Add(hit.transform.GetComponent<Renderer>());
            }           
            if(rendererCache.Count > 2)
            {
                propertyBlock.SetVector(PosId, new Vector4(view.x, view.y, view.z, -1f));
                rendererCache[0].SetPropertyBlock(propertyBlock);
                rendererCache.RemoveAt(0);

            }
            propertyBlock.SetVector(PosId, new Vector4(view.x, view.y, view.z, dissolveAmount));
            rendererCache[rendererCache.Count - 1].SetPropertyBlock(propertyBlock);            
            //WallDissolveMat.SetVector(PosId, new Vector4(view.x, view.y, view.z, dissolveAmount));
        }
        else
        {
            if (rendererCache.Count <= 0) return;
           

            foreach(Renderer render in rendererCache)
            {
                propertyBlock.SetVector(PosId, new Vector4(view.x, view.y, view.z, -1f));
                render.SetPropertyBlock(propertyBlock);
            }
            rendererCache.Clear();
        }
        //WallDissolveMat.SetVector(PosId, new Vector4(view.x, view.y, view.z, 1.6f));
    }

}
