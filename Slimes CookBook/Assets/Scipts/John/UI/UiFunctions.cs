using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UiFunctions : MonoBehaviour
{
    [SerializeField]
    GameObject UiPanel;
    [SerializeField]
    Button _Selected;
    bool on = false;
    public void OpenClose(InputAction.CallbackContext context)
    {
       
        on = !UiPanel.activeInHierarchy;
        UiPanel.SetActive(on);
        if (UiPanel.activeInHierarchy)
        {
            _Selected.Select();
        }
    }
}
