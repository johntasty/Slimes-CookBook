using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class DoorUnlock : NetworkBehaviour
{
    [SerializeField]
    int[] doorCode;
    [SerializeField]
    float OpenAniTime;
    bool opening = false;
    // Start is called before the first frame update
    void Start()
    {
        KeypadScreen.OnDoorUnlock += UnlockDoor;
    }

    private void UnlockDoor(int[] obj)
    {
        if(obj.Length != doorCode.Length) {return;}

        for (int i = 0; i < obj.Length; i++)
        {
            if (obj[i] != doorCode[i])
            {
               
                return;
            }               
        }
        CmdOpen();

    }
    [Command(requiresAuthority = false)]
    void CmdOpen()
    {
        if (opening) return;
        StartCoroutine(OpenDoor());
    }
    IEnumerator OpenDoor()
    {
        opening = true;
        float timeElapsed = 0;
        Quaternion openDoor = Quaternion.Euler(0, 90f, 0);
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / OpenAniTime;

            transform.localRotation = Quaternion.Slerp(transform.localRotation, openDoor, normalizedTime);
            yield return null;
        } while (timeElapsed < OpenAniTime);
        opening = false;
    }
}
