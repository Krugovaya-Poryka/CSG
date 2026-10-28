// Reads the existing army API; never creates or changes units.
public static class GridHeroArmyView
{
    public static string GetSlotText(HeroArmy army, int index)
    {
        if (army == null || army.slots == null || index < 0 || index >= army.slots.Length)
            return "Empty";

        ArmySlot slot = army.slots[index];
        if (slot.unitData == null || slot.count <= 0)
            return "Empty";

        string unitName = string.IsNullOrEmpty(slot.unitData.unitName)
            ? slot.unitData.name : slot.unitData.unitName;
        return unitName + " x " + slot.count;
    }
}
