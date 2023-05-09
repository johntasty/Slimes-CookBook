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
        if (wizard != null || slime != null)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                IncreaseSpeed();
            }
            else if (!Input.GetKeyDown(KeyCode.V) && elevatorMove.elevatorSpeed > 0.1f)
            {
                DecreaseSpeed();


            }
        }

    }

    //Assign player gameobjects
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wizard"))
            wizard = other.gameObject;
        if (other.gameObject.CompareTag("Slime"))
            slime = other.gameObject;


    }

    private void IncreaseSpeed()
    {
        if (wizard != null || slime != null)
        {

            elevatorMove.elevatorSpeed += pushStrenght;



        }
    }

    private void DecreaseSpeed()
    { elevatorMove.elevatorSpeed = Mathf.Lerp(elevatorMove.elevatorSpeed, -deCell, deCell * Time.fixedDeltaTime); }






















}