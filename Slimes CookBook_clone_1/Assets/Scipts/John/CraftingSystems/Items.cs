
using UnityEngine;

[CreateAssetMenu(fileName = "Items", menuName = "ScriptableObjects/Item", order = 1)]
public class Items : ScriptableObject
{
    public string ItemName;
    public GameObject ItemPrefab;

}
