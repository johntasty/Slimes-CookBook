using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeSawTriggers : MonoBehaviour
{
    Transform previousParent = null;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            other.transform.parent.position = transform.position;

            foreach (Transform child in other.transform.parent)
            {
                child.position = transform.position; 
            }
            other.transform.parent.SetParent(transform);           
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wizard") || other.CompareTag("Slime"))
        {
            other.transform.parent.SetParent(null);           
        }
    }
}
