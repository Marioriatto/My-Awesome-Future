using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "NewItemData", menuName = "Data/Item Data")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public string description;
    public GameObject icon, prefab;
    public List<string> options;
    public int price;
    public bool isSaleable;
}
