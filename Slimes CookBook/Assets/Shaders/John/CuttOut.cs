using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttOut : MonoBehaviour
{
    [SerializeField] Material WallMat;

    private static int PosId = Shader.PropertyToID("windows");
   
    private static int WindowBounds = Shader.PropertyToID("windowsBounds");

    public Vector4[] windowss = new Vector4[6];
    public Vector4[] windowssBounds = new Vector4[6];
    public List<Transform> windowPos = new List<Transform>();
    public List<Vector4> windowBoundsList = new List<Vector4>();

    private void Start()
    {
        windowss = new Vector4[6];
        windowssBounds = new Vector4[6];
        Cut();

    }
  
    void Cut()
    {

        for (int i = 0; i < windowPos.Count; i++)
        {
            windowss[i] = windowPos[i].position;
            windowssBounds[i] = windowBoundsList[i];
        }

        WallMat.SetVectorArray(WindowBounds, windowssBounds);
        WallMat.SetVectorArray(PosId, windowss);
       
    }
}
