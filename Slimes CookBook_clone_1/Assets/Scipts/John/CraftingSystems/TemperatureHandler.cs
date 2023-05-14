using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureHandler
{   
    public Vector2 previous;
   
    private Vector2[] inputs = new Vector2[4];
    private int currentIndex = 0;

    private float angle = 0;
    private float Sinangle = 0;
    private float mag = 0;
   
    public float _Temperature(Vector2 _Input)
    {
        if (_Input == Vector2.zero) return angle;
        Vector2 currentJoystick = new Vector2(_Input.x, _Input.y);

        mag = currentJoystick.magnitude;
        if (mag < 0.94f) return angle;
        inputs[currentIndex] = currentJoystick;

        if (currentJoystick != previous)
        {
            currentIndex = (currentIndex + 1) % inputs.Length;
            if (currentIndex == 0)
            {
                Vector2 averagePosition = Vector2.zero;
                for (int i = inputs.Length - 3; i < inputs.Length - 1; i++)
                {
                    averagePosition += inputs[i];
                }
                averagePosition /= 2f;

                Vector2 nextAveragePosition = Vector2.zero;
                for (int i = inputs.Length - 3; i < inputs.Length - 1; i++)
                {
                    nextAveragePosition += inputs[i + 1];
                }
                nextAveragePosition /= 2f;
                               
                Sinangle = Vector2.SignedAngle(nextAveragePosition ,averagePosition);
                angle += Sinangle;
                              
            }
           
        }

        previous = currentJoystick;
        return angle;
    }
   
}
