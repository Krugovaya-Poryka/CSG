using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    public int id;
    public string teamName;

    public List<HeroController> heroes = new List<HeroController>();
    public List<Castle> castles = new List<Castle>();
    public List<Mine> mines = new List<Mine>();

    public int gold;
    public int wood;
    public int ore;
    public int mercury;
    public int sulfur;
    public int crystal;
    public int gems;

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Gold: return gold;
            case ResourceType.Wood: return wood;
            case ResourceType.Ore: return ore;
            case ResourceType.Mercury: return mercury;
            case ResourceType.Sulfur: return sulfur;
            case ResourceType.Crystal: return crystal;
            case ResourceType.Gems: return gems;
            default: return 0;
        }
    }

    public bool AddResource(ResourceType type, int amount)
    {
        if (amount <= 0 || !ResourceCatalog.IsValid(type)) return false;
        // Avoid overflow without changing the existing serialized balance fields.
        if (GetResource(type) > int.MaxValue - amount) return false;
        switch (type)
        {
            case ResourceType.Gold: gold += amount; break;
            case ResourceType.Wood: wood += amount; break;
            case ResourceType.Ore: ore += amount; break;
            case ResourceType.Mercury: mercury += amount; break;
            case ResourceType.Sulfur: sulfur += amount; break;
            case ResourceType.Crystal: crystal += amount; break;
            case ResourceType.Gems: gems += amount; break;
        }
        return true;
    }
}
