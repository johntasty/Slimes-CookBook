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
    GameObject test;
    public LineRenderer lineRenderer;
    public float elevatorSpeed;
    int currentDes = 0;
    bool move;
    [SerializeField] private int elevatorDesLength;
    [SerializeField] private Vector3[] elevatorDes = new Vector3[0];
    public bool startPoint = false;
    public bool endPoint = false;
    //Vector3 myLocation;

    // Start is called before the first frame update
    void Start()
    {
        test = GameObject.FindWithTag("elevatorpath");
        elevatorDes = new Vector3[elevatorDesLength];
        lineRenderer = test.GetComponent<LineRenderer>();
        lineRenderer.positionCount = elevatorDes.Length;
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

            Vector3 moveLocations;
            moveLocations = elevatorDes[currentDes];
            if (this.transform.position != elevatorDes[currentDes])
            {
                this.transform.position = Vector3.MoveTowards(this.transform.position, moveLocations, elevatorSpeed * Time.deltaTime);
                if (this.transform.position == elevatorDes[currentDes]) currentDes++;

            }
            else if (currentDes >= elevatorDes.Length) { this.transform.position = elevatorDes[currentDes]; currentDes = 0; }

            Debug.Log(currentDes + "move locations");
            //WhereElevator();
        }

        //WhereElevator();
    }
    
    public void WhereElevator()
    {
         Vector3 startPos = elevatorDes[0];
        Vector3 endPos = elevatorDes[elevatorDesLength];
        if (this.transform.position == startPos)
        {
            startPoint = true;
        }
        else { startPoint = false; }

        if (this.transform.position == endPos)
        {
            endPoint = true;

        }
        else { endPoint = false; }
    }    

}
