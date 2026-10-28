using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Team
{
    public int id;
    public string teamName;

    public List<HeroController> heroes = new List<HeroController>();
    public List<Castle> castles = new List<Castle>();

    public int gold;
    public int wood;
    public int ore;
}