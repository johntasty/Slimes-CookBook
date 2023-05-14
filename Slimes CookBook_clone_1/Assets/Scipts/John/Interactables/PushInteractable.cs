using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PushInteractable : MonoBehaviour, IPushable
{
    private Vector3 origin;
    [SerializeField]
    float maxPushX;

    bool InteractingNow;
  
    // Start is called before the first frame update
    void Start()
    {
        origin = transform.position;
    }
    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("Wizard"))
        {

           PushObject(1f, other.transform.position);

        }
    }

   
    public void PushObject(float force, Vector3 direction)
    {
        if (!InteractingNow) return;
        Vector3 dir = transform.position - direction;
        dir.y = 0;
        dir.z = 0;
        dir.Normalize();
        Vector3 pushed = transform.position + dir * force * Time.deltaTime;
        pushed.x = Mathf.Clamp(pushed.x, origin.x - maxPushX, origin.x + maxPushX);

        transform.position = pushed;
    }

    public void Interacting(bool Interact)
    {
        InteractingNow = Interact;
    }
}
