using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRopePoint
{
    public void SetUpSettings(float len, float damp, float springCon, float restLength);
       
    public void SimulateJoint(float gravity, float time);
}
