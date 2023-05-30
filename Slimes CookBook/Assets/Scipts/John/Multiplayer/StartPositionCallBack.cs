using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPositionCallBack : MonoBehaviour
{
    [SerializeField] string SceneName;
    public void OnEnable()
    {
        SceneName = gameObject.scene.name;

        AdditiveNetwork.RegisterTeleportPositions(transform, SceneName);
    }

    public void OnDestroy()
    {
        AdditiveNetwork.UnRegisterTeleportPositions(SceneName);
    }
}
