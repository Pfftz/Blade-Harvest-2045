using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item Data", menuName = "Item Data", order = 50)]
public class ItemData : ScriptableObject
{
    [Header("Basic Information")]
    public string itemName = "Item Name";
    public Sprite icon;
    [TextArea(3, 5)]
    public string description = "Item description";
    
    [Header("Shop Settings")]
    public int buyPrice = 10;
    public bool canBuy = true;
    public bool canSell = true;
    
    [Header("Category")]
    public ItemCategory category = ItemCategory.Misc;
    
    public enum ItemCategory
    {
        Seed,
        Crop,
        Tool,
        Material,
        Misc
    }
}