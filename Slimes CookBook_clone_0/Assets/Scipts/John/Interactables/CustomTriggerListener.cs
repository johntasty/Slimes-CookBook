using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomTriggerListener : MonoBehaviour
{
    public event Action OnTriggerEntered;
    public event Action OnTriggerExited;

    [Header("What objects to interact with, \n use objects tag.")]
    [SerializeField]
    string InteractionTag;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(InteractionTag))
        {
            OnTriggerEntered?.Invoke();
        }       
    }
}
