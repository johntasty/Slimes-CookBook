using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftMechanism : MonoBehaviour
{
    [SerializeField] Transform lever;

    public bool wizardLever;
    public bool slimeLever;

    Vector3 currentEulerAngles;
    Quaternion currentRotation;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame

    public void LeverRotation()
    {
        

        if(wizardLever) { currentEulerAngles = new Vector3(20, 0, 0); slimeLever = true; }
        if (slimeLever) { currentEulerAngles = new Vector3 (-20,0, 0); wizardLever = true; }

        currentRotation.eulerAngles = currentEulerAngles;
    }
}
