using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class PropertyBLock : MonoBehaviour
{
    [SerializeField]
    Material FoliageMat;

    [SerializeField]
    Color color1;
    [SerializeField]
    Color color2;
    [SerializeField]
    Color Outline;

    private static int _Color1 = Shader.PropertyToID("_Color");
    private static int _Color2 = Shader.PropertyToID("_Color2");
    private static int _Outline = Shader.PropertyToID("_Outline");

    private MaterialPropertyBlock propertyBlock;
    [SerializeField]
    public Renderer rendererCache;
    private void LateUpdate()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        propertyBlock.SetColor(_Color1, color1);
        propertyBlock.SetColor(_Color2, color2);
        propertyBlock.SetColor(_Outline, Outline);
        rendererCache.SetPropertyBlock(propertyBlock);
    }
}
