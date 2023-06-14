using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class ElevatorButtons : NetworkBehaviour
{
    public static Action MoveElevatorSuccess;
    public static Action ReturnJourney;
    [SerializeField]
    float duration = .8f;
    [SerializeField]
    Transform SlimeButton;
    Vector3 buttonPosition;
    [SerializeField]
    Transform WizardLever;
    [SerializeField]
    Transform target1;
    [SerializeField]
    Transform target2;
    public float dir;
    public float angleCap;
    
    string interactor;   
    string prevInteractor;
    Coroutine rotator = null;

    [SyncVar]
    public int succesfull = 0;

    private void Start()
    {
        ButtonTriggers.TriggerPosition += CmdRotator;
        buttonPosition = SlimeButton.localPosition;
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
   
    [Command(requiresAuthority = false)]
    void CmdRotator(Vector3 targetSide, string Interactor)
    {
        StopCoroutine(rotator);       
        interactor = Interactor;
        StartRotator();
    }
    void StartRotator()
    {
        if (interactor == null) return;
        rotator = StartCoroutine(SeaSaw());
    }
    [Command(requiresAuthority = false)]
    void CmdSuccessRotation()
    {       
        succesfull = 0;
        MoveElevatorSuccess?.Invoke();        
    }
    [Command(requiresAuthority = false)]
    public void CmdReturnJourney()
    {
        ReturnJourney?.Invoke();
    }
    IEnumerator SeaSaw()
    {
       if(interactor == "Slime")
        {
            yield return StartCoroutine(PressurePlate());
        }
        else
        {
            yield return StartCoroutine(WheelRotation());
        }
        if (interactor != prevInteractor)
        {
            CmdIncrementSuccess();
            prevInteractor = interactor;
        }
        if (succesfull >= 2)
        {
            CmdSuccessRotation();
        }
            
    }
    IEnumerator PressurePlate()
    {      
        Vector3 endPosition = new Vector3(buttonPosition.x, buttonPosition.y - .1f, buttonPosition.z);
        float time = 0;
           
        do
        {
            time += Time.deltaTime;
            float normalTime = time / duration;
            SlimeButton.localPosition = Vector3.Lerp(buttonPosition, endPosition, normalTime);

            yield return null;
        } while (time < duration);
        do
        {
            time += Time.deltaTime;
            float normalTime = time / duration;
            SlimeButton.localPosition = Vector3.Lerp(SlimeButton.localPosition, buttonPosition, normalTime);

            yield return null;
        } while (time < duration);

    }
    IEnumerator WheelRotation()
    {
        float time = 0;             
        float duration = 1f;
        do
        {
            time += Time.deltaTime;
            float normalTime = time / duration;
            float angle = (360f * normalTime);
            Quaternion RotateAngle = Quaternion.AngleAxis(angle * 3, Vector3.up);
            WizardLever.localRotation = Quaternion.Slerp(WizardLever.localRotation, RotateAngle, normalTime);
            yield return null;

        } while (time < duration);
   
    }
    [Command(requiresAuthority = false)]
   void CmdIncrementSuccess()
    {
        succesfull++;
    }
}
