using UnityEngine;

public enum ResourceType { Gold, Wood, Ore, Mercury, Sulfur, Crystal, Gems }

// Shared presentation data; balances still belong to the existing Team.
public static class ResourceCatalog
{
    public const int Count = 7;
    private static readonly string[] Names = { "Gold", "Wood", "Ore", "Mercury", "Sulfur", "Crystal", "Gems" };
    private static readonly string[] Files = { "gold", "wood", "ore", "mercury", "sulfur", "crystal", "gems" };
    private static readonly Sprite[] Icons = new Sprite[Count];

    public static bool IsValid(ResourceType type) => (int)type >= 0 && (int)type < Count;
    public static string GetName(ResourceType type) => IsValid(type) ? Names[(int)type] : "Resource";

    public static Sprite GetSprite(ResourceType type)
    {
        if (!IsValid(type)) return null;
        int index = (int)type;
        if (Icons[index] == null)
            Icons[index] = Resources.Load<Sprite>("StrategyResources/" + Files[index]);
        return Icons[index];
    }

    public static int GetPickupAmount(ResourceType type, System.Random random)
    {
        if (type == ResourceType.Gold) return random.Next(5, 11) * 100;
        return type == ResourceType.Wood || type == ResourceType.Ore
            ? random.Next(5, 11) : random.Next(3, 7);
    }
}
