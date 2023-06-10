using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPush : MonoBehaviour
{
    [SerializeField]
    GameObject plank;
    //private void OnTriggerEnter(Collider other)
    //{
    //    bool foundInteractable = other.TryGetComponent(out IPushable InteractObject);
    //    if (foundInteractable)
    //    {           
    //        InteractObject.Interacting(true);
    //    }
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    bool foundInteractable = other.TryGetComponent(out IPushable InteractObject);
    //    if (foundInteractable)
    //    {
    //        InteractObject.Interacting(false);
    //    }
    //}
    private void Start()
    {
        plank.SetActive(true);
        MoveObject.OnHoleSelected += CmdMove;
    }
    
    private void CmdMove(int obj)
    {
        Debug.Log("scene2");

        //Vector3 targetButton = holesHolder.buttons[obj].transform.position;
        //StartCoroutine(MoveObj(targetButton));
    }
}
