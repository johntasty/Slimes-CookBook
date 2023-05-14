using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeypadScreen : MonoBehaviour
{
    [SerializeField]
    List<GameObject> symbols;
    [SerializeField]
    Transform SymbolsScreen;
    [SerializeField]
    int DigitsNumber;
    
    List<GameObject> displaySymbols = new List<GameObject>();

    public int[] codeSubmitions;
    //just a work around cause am tired
    List<int> codeNumbers = new List<int>();
    public static event Action<int[]> OnDoorUnlock;
    // Start is called before the first frame update
    void Start()
    {
        codeSubmitions = new int[DigitsNumber];
        MoveObject.OnDelete += DeleteLast;
        MoveObject.OnNumberSelected += SetNumber;
        MoveObject.OnSubmit += SubmitCode;
    }

    private void SubmitCode()
    {   
        for(int i = 0; i < codeNumbers.Count; i++)
        {
            codeSubmitions[i] = codeNumbers[i];
        }
        OnDoorUnlock?.Invoke(codeSubmitions);
    }

    private void SetNumber(int obj)
    {
      
        if (displaySymbols.Count > DigitsNumber - 1) return;
        GameObject symbol = Instantiate(symbols[obj - 1], SymbolsScreen);
        symbol.SetActive(true);
        displaySymbols.Add(symbol);
        codeNumbers.Add(obj);
    }

    private void DeleteLast()
    {
        if (displaySymbols.Count <= 0) return;
        GameObject temp = displaySymbols[displaySymbols.Count - 1];
        displaySymbols.RemoveAt(displaySymbols.Count - 1);
        codeNumbers.RemoveAt(codeNumbers.Count - 1);
        Destroy(temp);
    }
       
}
