using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRopePoint
{
    public void SetUpSettings(float spd, float weight, float damp, float springCon, float restLength, RopeConstruct rope);
       
    public void SimulateJoint(float gravity, float time);
}
