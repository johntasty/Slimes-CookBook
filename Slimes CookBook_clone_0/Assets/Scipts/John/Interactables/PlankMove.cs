using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlankMove : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MoveObject.OnHoleSelected += CmdMove;
    }

    [Command (requiresAuthority = false)]
    private void CmdMove(Vector3 obj)
    {
       
        transform.rotation = Quaternion.identity;
        transform.position = obj;
    }
}
