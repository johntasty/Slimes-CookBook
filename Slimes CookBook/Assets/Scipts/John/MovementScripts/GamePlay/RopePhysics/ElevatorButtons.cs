using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class ElevatorButtons : NetworkBehaviour
{
    public static Action MoveElevatorSuccess;
    [SerializeField]
    float duration = 1f;
    [SerializeField]
    Transform SeSawObject;
    [SerializeField]
    Transform target1;
    [SerializeField]
    Transform target2;
    public float dir;
    public float angleCap;
    Vector3 target;
    Vector3 prevtarget;
    Coroutine rotator = null;
    int succesfull = 0;
    private void Start()
    {
        ButtonTriggers.TriggerPosition += CmdRotator;
        target = target1.position;
        prevtarget = target2.position;
        rotator = StartCoroutine(SeaSaw());
        StopCoroutine(rotator);
    }
   public bool CheckLocal(Transform player)
   {
       
        bool local = player.GetComponent<NetworkIdentity>().isLocalPlayer;
        return local;
   }
    private void OnDestroy()
    {
        ButtonTriggers.TriggerPosition -= CmdRotator;
    }
    private void Update()
    {
        CmdSuccessRotation();
       
    }
    [Command(requiresAuthority = false)]
    void CmdRotator(Vector3 targetSide)
    {
        StopCoroutine(rotator);
        target = targetSide;
        StartRotator();
    }
    void StartRotator()
    {
        if (target == null) return;
        rotator = StartCoroutine(SeaSaw());
    }
    [Command(requiresAuthority = false)]
    void CmdSuccessRotation()
    {
        if (succesfull < 2) return;
        succesfull = 0;
        MoveElevatorSuccess?.Invoke();        
    }
    IEnumerator SeaSaw()
    {
       
        float time = 0;
        Vector3 dirs = (target - SeSawObject.position).normalized;
        float dot = Vector3.Dot(dirs, transform.forward);
        dir = Mathf.Sign(dot);
        Quaternion rotateTowards = Quaternion.AngleAxis(angleCap * dir, Vector3.right);
        do
        {
            time += Time.deltaTime;
            float normalTime = time / duration;
            SeSawObject.localRotation = Quaternion.Slerp(SeSawObject.localRotation, rotateTowards, normalTime);

            yield return null;
        } while (time < duration);
        if(target != prevtarget)
        {
            succesfull++;            
            prevtarget = target;
        }
        
    }
   
}
