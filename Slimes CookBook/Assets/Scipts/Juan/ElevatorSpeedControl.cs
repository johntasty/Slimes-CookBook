using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorSpeedControl : MonoBehaviour
{
    /// <summary>
    /// push strenght is how much elevator will move
    /// deCell is how fast elevator will move backwards
    /// 
    /// </summary>

    [SerializeField] GameObject wizard;
    [SerializeField] GameObject slime;

    [SerializeField] private ElevatorMoveTo elevatorMove;
    [SerializeField] private LiftMechanism liftScript;

    [SerializeField] private float pushStrenght;
    [SerializeField] private float deCell;

    bool accelarating = false;
    bool deccelaration = false;

    // Start is called before the first frame update
    void Start()
    {
        //wizard = GameObject.FindWithTag("Player");
        //slime = GameObject.FindWithTag("Slime");

        elevatorMove = GetComponentInParent<ElevatorMoveTo>();

    }


    void Update()
    {
        //Debug.Log(accelarating + " accelerating");
        ////Debug.Log(pushStrenght + "pushStrenght");
        //Debug.Log(elevatorMove.elevatorSpeed + "speed");
        if (Input.GetKeyDown(KeyCode.V))
        {
            if(liftScript.wizardLever)
            { AccelaratingNow(); }
            //AccelaratingNow();
        }



    }
    IEnumerator Accelarate(Vector3 target)
    {
        
        accelarating = true;
       
        float strenght = pushStrenght;
        float duration = 2f;
        float time = 0;
        Vector3 targetDirection = (target - transform.position).normalized * strenght;
        Vector3 targ = transform.position + targetDirection;
        do {
            time += Time.deltaTime;
            float normalizedTime = time / duration;
            transform.position = Vector3.Lerp(transform.position, targ , normalizedTime);
            yield return null;
        } while (time < duration);

        accelarating = false;
        
    }
    void AccelaratingNow()
    {
        if (accelarating) return;
        StartCoroutine(Accelarate(elevatorMove.moveLocations));
    }
    //Assign player gameobjects
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wizard") || other.gameObject.CompareTag("Slime"))
        {
            //wizard = other.gameObject;
            //wizard.transform.SetParent(transform, false);
            other.transform.SetParent(transform, false);
            //other.gameObject.transform.SetParent(transform, false);
            //ElevatorCollision();
        }
    }

    private void IncreaseSpeed()
    {
        if (wizard != null || slime != null)
        {
            float currentVelocity = 0f;

            //elevatorMove.elevatorSpeed = Mathf.SmoothDamp(elevatorMove.elevatorSpeed, pushStrenght, ref currentVelocity, 2f, pushStrenght);
            //elevatorMove.elevatorSpeed = Mathf.Lerp(elevatorMove.elevatorSpeed, pushStrenght, );

            //pushStrenght += Mathf.SmoothDamp(pushStrenght, pushStrenght, ref elevatorMove.elevatorSpeed, 0.1f * Time.fixedDeltaTime);
            //elevatorMove.elevatorSpeed += pushStrenght;


        }
    }

    private void DecreaseSpeed()
    {
        float currentVelocity = 0.0f;

        //elevatorMove.elevatorSpeed = Mathf.Lerp(elevatorMove.elevatorSpeed, -deCell, deCell * Time.fixedDeltaTime);

        elevatorMove.elevatorSpeed = Mathf.SmoothDamp(elevatorMove.elevatorSpeed,0, ref currentVelocity, deCell * Time.fixedDeltaTime);

    }



    private void ElevatorCollision()
    {
        if (wizard != null) 
        {
            wizard.transform.SetParent(this.transform, false);
        } else { wizard.transform.SetParent(null, false); }
    }    


















}