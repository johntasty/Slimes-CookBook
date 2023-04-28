using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Linq;
[ExecuteInEditMode]
public class RendererController : MonoBehaviour
{
    [SerializeField] UniversalRendererData rendererData = null;
    [SerializeField] string featureName = null;

    [SerializeField] List<Transform> positions = new List<Transform>();
    [SerializeField] Light directionalLight;
    [SerializeField] float blendStrenght;
    [SerializeField] float size;
    [SerializeField] int numShapes;
    Shape[] shapeData;
   
    void Start()
    {
        shapeData = new Shape[numShapes];
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i].position;
            shapeData[i].position = pos;
        }
    }

    // Update is called once per frame
    void Update()
    {      
        for (int i = 0; i < positions.Count; i++)
        {
            Vector3 pos = positions[i].position;
            shapeData[i].position = pos;
        }

        UpdateCompute();
    }
    private bool TryGetFeature(out ScriptableRendererFeature feature)
    {
        feature = rendererData.rendererFeatures.Where((f) => f.name == featureName).FirstOrDefault();
       
        return feature != null;
    }
    private void UpdateCompute()
    {
        if(TryGetFeature(out var feature))
        {
            var test = feature as FullscreenCompute;
            test.settings.blendStrength = blendStrenght;
            test.settings.size = size;
            test.settings.numShapes = numShapes;            
            test.settings.shapeData = shapeData;


        }
       
    }
}
