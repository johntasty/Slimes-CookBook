using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPushable
{
    void PushObject(float force, Vector3 direction);
    void Interacting(bool Interact);
}
