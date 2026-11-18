using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HeroInventory
{
    public List<Item> items = new List<Item>();

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public void RemoveItem(Item item)
    {
        items.Remove(item);
    }
}