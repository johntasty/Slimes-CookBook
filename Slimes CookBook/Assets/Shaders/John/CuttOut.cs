using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CuttOut : MonoBehaviour
{
    [SerializeField] Material WallMat;

    private static int PosId = Shader.PropertyToID("windows");
   
    private static int WindowBounds = Shader.PropertyToID("windowsBounds");
    //hard coded to 6 since that al the windows we had
    public Vector4[] windowss = new Vector4[6];
    public Vector4[] windowssBounds = new Vector4[6];
    //all the windows in the scene
    public List<Transform> windowPos = new List<Transform>();
    //each windows might have different proportions, they can be customized here
    public List<Vector4> windowBoundsList = new List<Vector4>();

    private void Start()
    {
        windowss = new Vector4[6];
        windowssBounds = new Vector4[6];
        Cut();

    }
  
    void Cut()
    {
        //all the windows within the scene
        for (int i = 0; i < windowPos.Count; i++)
        {
            windowss[i] = windowPos[i].position;
            windowssBounds[i] = windowBoundsList[i];
        }

        WallMat.SetVectorArray(WindowBounds, windowssBounds);
        WallMat.SetVectorArray(PosId, windowss);
       
    }
}
