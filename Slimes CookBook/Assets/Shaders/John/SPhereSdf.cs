using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SPhereSdf : MonoBehaviour
{
    public Material material;

    private static int SpherePosition = Shader.PropertyToID("_Sphere1");
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (material == null) return;
        material.SetVector(SpherePosition, transform.position);
        
    }
}
