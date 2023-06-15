using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeIntegration : MonoBehaviour
{
    //this handles all the simulations
    //different ropes can be added 
    //all are made individually and updated individually
    //gives flexibility in having all types of ropes, real time or baked
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
