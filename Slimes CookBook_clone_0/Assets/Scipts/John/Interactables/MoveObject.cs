using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class MoveObject : MonoBehaviour, ISelectHandler
{
    public static event Action<int> OnHoleSelected;
    public static event Action<int> OnHoleHighLight;
    public static event Action<int> OnNumberSelected;

    public static event Action OnSubmit;
    public static event Action OnDelete;

    private void Start()
    {
        enabled = true;
        gameObject.SetActive(true);
    }

    
    public void CmdMoveTohole()
    {

        OnHoleSelected?.Invoke(int.Parse(gameObject.name));
    }

    public void DisplayImage()
    {
        OnNumberSelected?.Invoke(int.Parse(gameObject.name));
    }
    public void DeleteOne()
    {
        OnDelete?.Invoke();
    }
    public void EnterCode()
    {
        OnSubmit?.Invoke();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(int.TryParse(gameObject.name, out int result))
        {
            OnHoleHighLight?.Invoke(result);
        }
       
    }
}
