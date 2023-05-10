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

    [SerializeField] private float pushStrenght;
    [SerializeField] private float deCell;

    // Start is called before the first frame update
    void Start()
    {
        //wizard = GameObject.FindWithTag("Player");
        slime = GameObject.FindWithTag("slime");

        elevatorMove = GetComponentInParent<ElevatorMoveTo>();




    }






    void Update()
    {
        //Debug.Log(pushStrenght + "pushStrenght");
        //Debug.Log(elevatorMove.elevatorSpeed + "speed");
        if (wizard != null || slime != null)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                IncreaseSpeed();
                //elevatorMove.elevatorSpeed = pushStrenght;
            }
            else if (!Input.GetKeyDown(KeyCode.V) && elevatorMove.elevatorSpeed > 0f)
            {
                DecreaseSpeed();


            }
        }

    }

    //Assign player gameobjects
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Wizard"))
            wizard = other.gameObject;
        if (other.gameObject.CompareTag("Slime"))
            slime = other.gameObject;
        ElevatorCollision();

    }

    private void IncreaseSpeed()
    {
        if (wizard != null || slime != null)
        {
            float currentVelocity = 0f;

            elevatorMove.elevatorSpeed += Mathf.SmoothDamp(elevatorMove.elevatorSpeed, pushStrenght, ref currentVelocity, 10f);

            //pushStrenght += Mathf.SmoothDamp(pushStrenght, pushStrenght, ref elevatorMove.elevatorSpeed, 0.1f * Time.fixedDeltaTime);
            elevatorMove.elevatorSpeed += pushStrenght;


        }
    }

    private void DecreaseSpeed()
    {
        float currentVelocity = 0.0f;

        //elevatorMove.elevatorSpeed = Mathf.Lerp(elevatorMove.elevatorSpeed, -deCell, deCell * Time.fixedDeltaTime);

        elevatorMove.elevatorSpeed = Mathf.SmoothDamp(elevatorMove.elevatorSpeed,0, ref currentVelocity, deCell * Time.deltaTime);

    }



    private void ElevatorCollision()
    {
        if (wizard != null) 
        {
            wizard.transform.SetParent(this.transform, false);
        } else { wizard.transform.SetParent(null, false); }
    }    


















}