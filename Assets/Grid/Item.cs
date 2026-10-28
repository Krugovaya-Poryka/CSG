using UnityEngine;

public enum EquipmentSlot
{
    None,
    Weapon,
    Shield,
    Helmet,
    Armor
}

[System.Serializable]
public class Item
{
    public string itemName;
    public EquipmentSlot slot;
    public int attackBonus;
    public int defenseBonus;
}
