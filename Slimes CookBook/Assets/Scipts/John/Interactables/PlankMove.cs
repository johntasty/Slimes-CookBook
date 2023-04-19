using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlankMove : NetworkBehaviour
{
    Vector3 StartPosition;
    // Start is called before the first frame update
    void Start()
    {
        StartPosition = transform.position;

        MoveObject.OnHoleSelected += CmdMove;
        MoveObject.OnHoleHighLight += CmdRotate;
    }
    private void OnDisable()
    {
        MoveObject.OnHoleSelected -= CmdMove;
        MoveObject.OnHoleHighLight -= CmdRotate;
    }
    [Command(requiresAuthority = false)]
    private void CmdRotate(Vector3 obj)
    {
        if (transform.position != StartPosition) return;
        StartCoroutine(RotatePlank(obj));
    }

    [Command (requiresAuthority = false)]
    private void CmdMove(Vector3 obj)
    {       
        StartCoroutine(MoveObj(obj));       
    }
    IEnumerator RotatePlank(Vector3 target)
    {        
        float timeElapsed = 0;
        Vector3 direction = target - transform.position;
        Quaternion hole = Quaternion.LookRotation(direction);
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / 1;
            transform.rotation = Quaternion.Lerp(transform.rotation, hole, normalizedTime);
            yield return null;
        } while (timeElapsed < 1);
       
    }
    IEnumerator MoveObj(Vector3 target)
    {
        if(transform.position != StartPosition)
        {
            Vector3 startPoint = transform.position;
            Vector3 centerPoint = (startPoint + StartPosition) / 2;

            Vector3 startDirection = centerPoint - startPoint;
            Vector3 endDirection = target - centerPoint;
            float timeElapsedBack = 0;

            do
            {
                timeElapsedBack += Time.deltaTime;
                float normalizedTime = timeElapsedBack / 2;
                Quaternion StartRot = Quaternion.LookRotation(startDirection);         
                Quaternion EndRot = Quaternion.LookRotation(endDirection);         
                
                // Quadratic bezier curve
                transform.position =
                  Vector3.Lerp(
                    Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                    Vector3.Lerp(centerPoint, target, normalizedTime),
                    normalizedTime
                  );
                transform.rotation = Quaternion.Slerp(
                    Quaternion.Slerp(transform.rotation, StartRot, normalizedTime), 
                    Quaternion.Slerp(transform.rotation, EndRot, normalizedTime), 
                    normalizedTime);
                yield return null;
            } while (timeElapsedBack < 2);
        }

        float timeElapsed = 0;       
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / 1;
           
            transform.position = Vector3.Lerp(transform.position, target, normalizedTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.identity, normalizedTime);
            yield return null;
        } while (timeElapsed < 1);
    }
}
