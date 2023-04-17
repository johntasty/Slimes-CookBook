using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObserverListener : MonoBehaviour
{

    public static ObserverListener Instance { get; private set; }

    SimpleControll Wizard;
    public SimpleControll _Wizard
    {
        set => Wizard = value;      
    }
    SimpleControll Slime;
    public SimpleControll _Slime
    {
        set => Slime = value;
    }
    public event Action<bool> InteractWizard;
    public event Action<bool> InteractSlime;
    private void Awake()
    {
        Instance = this;
       
    }

    public void WireUp()
    {
        Wizard.Interact += Interactacted;
    }
    public void WireUpSlime()
    {
        Slime.Interact += Interactacted;
    }
    private void Interactacted(bool obj)
    {
        InteractWizard?.Invoke(obj);
    }
}
