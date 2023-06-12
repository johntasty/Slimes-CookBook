using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraAdjuster : MonoBehaviour
{
    [SerializeField]
    int _CameraSize;
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Slime") || other.CompareTag("Wizard"))
        {
            other.transform.parent.GetComponentInChildren<CinemachineVirtualCamera>().m_Lens.OrthographicSize = _CameraSize;
        }
    }
}
