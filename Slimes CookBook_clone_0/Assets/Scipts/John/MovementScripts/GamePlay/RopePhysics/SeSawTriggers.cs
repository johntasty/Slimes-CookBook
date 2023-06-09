using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeSawTriggers : MonoBehaviour
{
    public SimpleControll controller = null;
    [SerializeField]
    testingPos platform;
    [SerializeField]
    Transform plat;
    public Vector3 lastPos;
    public float check;
    [SerializeField]
    ElevatorButtons sesawManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            if (!sesawManager.CheckLocal(other.transform.parent)) { return; }
            if (other.transform.parent.TryGetComponent(out controller))
            {
               
                controller.OnPlatform();
                controller.InjectPosition(plat);
              
            }
    
        }
    }
     
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            if (!sesawManager.CheckLocal(other.transform.parent)) { return; }
            if (controller != null)
            {
                controller.OnPlatform();
                controller = null;
               
            }
        }
    }
}
