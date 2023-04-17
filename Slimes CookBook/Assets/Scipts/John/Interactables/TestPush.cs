using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPush : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        bool foundInteractable = other.TryGetComponent(out IPushable InteractObject);
        if (foundInteractable)
        {           
            InteractObject.Interacting(true);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        bool foundInteractable = other.TryGetComponent(out IPushable InteractObject);
        if (foundInteractable)
        {
            InteractObject.Interacting(false);
        }
    }


}
