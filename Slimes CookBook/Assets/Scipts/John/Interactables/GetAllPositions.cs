using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetAllPositions : MonoBehaviour
{
    public Button[] buttons;
    public bool Close;
    // Start is called before the first frame update
    void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        if (Close)
        {
            gameObject.SetActive(false);
        }
        
        
    }
       
}
