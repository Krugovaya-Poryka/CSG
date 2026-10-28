using System.Collections.Generic;

[System.Serializable]
public class HeroInventory
{
    public List<Item> items = new List<Item>();
    public List<Item> equippedItems = new List<Item>();

    public void AddItem(Item item)
    {
        if (item == null)
            return;

        if (items == null)
            items = new List<Item>();

        if (!items.Contains(item) && (equippedItems == null || !equippedItems.Contains(item)))
            items.Add(item);
    }

    public bool RemoveItem(Item item)
    {
        return item != null && items != null && items.Remove(item);
    }

    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems?.Find(item => item != null && item.slot == slot);
    }

    public bool EquipItem(Item item)
    {
        if (item == null || item.slot <= EquipmentSlot.None || item.slot > EquipmentSlot.Armor ||
            items == null || !items.Contains(item))
            return false;

        if (equippedItems == null)
            equippedItems = new List<Item>();
        if (equippedItems.Contains(item))
            return false;

        // Replacing equipment returns the old item to the backpack.
        Item previous = GetEquippedItem(item.slot);
        if (previous != null)
            UnequipItem(previous);

        items.Remove(item);
        equippedItems.Add(item);
        return true;
    }

    public bool UnequipItem(Item item)
    {
        if (item == null || equippedItems == null || !equippedItems.Remove(item))
            return false;

        AddItem(item);
        return true;
    }

    public int AttackBonus => SumEquipmentBonus(true);
    public int DefenseBonus => SumEquipmentBonus(false);

    private int SumEquipmentBonus(bool attack)
    {
        int total = 0;
        if (equippedItems != null)
            foreach (Item item in equippedItems)
                if (item != null)
                    total += attack ? item.attackBonus : item.defenseBonus;
        return total;
    }
}
