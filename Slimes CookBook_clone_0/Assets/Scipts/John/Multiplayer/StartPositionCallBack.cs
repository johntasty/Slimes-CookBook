using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionCallBack : MonoBehaviour
{
    [SerializeField] string SceneName;
    [SerializeField] bool Checkpoint;
    public void OnEnable()
    {
        if (Checkpoint)
        {
            AdditiveNetwork.RegisterTeleportPositions(transform, transform.gameObject.name);
            return;
        }
        SceneName = gameObject.scene.name;

        AdditiveNetwork.RegisterTeleportPositions(transform, SceneName);
    }

    public void OnDestroy()
    {
        if (Checkpoint)
        {           
            AdditiveNetwork.UnRegisterTeleportPositions(transform.gameObject.name);
            return;
        }
        AdditiveNetwork.UnRegisterTeleportPositions(SceneName);
    }
}
