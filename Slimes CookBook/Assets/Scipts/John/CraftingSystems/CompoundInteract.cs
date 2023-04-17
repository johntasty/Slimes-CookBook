using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class CompoundInteract : MonoBehaviour, Iinteractable
{
    [SerializeField]
    Elements compound;
    [SerializeField]
    GameObject _Promt;
    [SerializeField]
    TMP_Text _Amount;
    [SerializeField]
    TMP_Text _Speed;

    public void GetCurrent(float amount, int Active)
    {
        if(Active == 0)
        {
            _Amount.text = amount.ToString("#.000");
        }
        else if(Active == 1)
        {
            _Speed.text = amount.ToString("#.000");
        }       
    }

    public string GetName()
    {
        return compound.CompoundName;
    }
    public void Promt(bool enable)
    {
        _Promt.SetActive(enable);
    }
       
}
