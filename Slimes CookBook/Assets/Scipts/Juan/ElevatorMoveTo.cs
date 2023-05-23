using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.FilePathAttribute;

public class ElevatorMoveTo : MonoBehaviour
{
    /// <summary>
    /// in editor elevator speed moves elevetaor along path
    /// elevatorDesLenght set it to the lenght of the line renderer points
    /// </summary>
    /// 

    //public List <GameObject> locations;
    [SerializeField] public CharacterController characterController;
    GameObject test;
    public LineRenderer lineRenderer;
    public float elevatorSpeed;
    int currentDes = 0;
    bool move;

    [SerializeField] public Vector3[] elevatorDes = new Vector3[0];
    public Vector3 moveLocations;
    public bool startPoint = false;
    public bool endPoint = false;

    //Vector3 myLocation;

    // Start is called before the first frame update
    void Start()
    {
        test = GameObject.FindWithTag("elevatorpath");
        
        lineRenderer = test.GetComponent<LineRenderer>();
        elevatorDes = new Vector3[lineRenderer.positionCount];
        lineRenderer.GetPositions(elevatorDes);

    }

    // Update is called once per frame
    void Update()
    {
        MoveElevator();

    }

    //takes vector3 array index vector positions and passes them to script gameobject to move it
    public void MoveElevator()
    {
        if (currentDes < elevatorDes.Length)
        {

            //Vector3 moveLocations;
            Vector3 currentVelocity = Vector3.zero;
            moveLocations = elevatorDes[currentDes];
            Vector3 direction = moveLocations - transform.position;
            if (this.transform.position != elevatorDes[currentDes])
            {
                characterController.Move(direction * elevatorSpeed * Time.deltaTime);

                
                if (this.transform.position == elevatorDes[currentDes]) currentDes++;

            }
            else if (currentDes >= elevatorDes.Length) { this.transform.position = elevatorDes[currentDes]; currentDes = 0; }

           
        }

    }

   
}
