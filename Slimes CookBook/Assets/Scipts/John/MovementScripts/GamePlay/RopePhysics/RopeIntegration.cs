using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeIntegration : MonoBehaviour
{
    [SerializeField]
    List<RopeConstruct> ropes;

    private void Start()
    {
        foreach(RopeConstruct rope in ropes)
        {
            rope.SimStartup();
        }
    }
}
