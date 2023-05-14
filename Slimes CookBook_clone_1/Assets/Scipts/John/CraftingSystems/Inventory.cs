using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    List<Elements> _Materials;
    [SerializeField]
    Dictionary<string, Object> _Elements = new Dictionary<string, Object>();
    private void Awake()
    {
        foreach(Elements element in _Materials)
        {
            _Elements.Add(element.CompoundName, element);
        }
    }
    public Dictionary<string, Object> Elements
    {
        get
        {
            return _Elements;
        }        
    }
    public List<Elements> Materials
    {
        get { return _Materials; }
    }
    public void AddMat(Elements inventoryItem)
    {
        _Materials.Add(inventoryItem);
    }

}
