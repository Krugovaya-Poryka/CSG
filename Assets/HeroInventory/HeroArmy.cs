using System;
using UnityEngine;

[Serializable]
public struct ArmySlot
{
    public UnitData unitData; // Файл характеристик (Pikeman, Angel тощо)
    public int count;         // Кількість істот
}

public class HeroArmy : MonoBehaviour
{
    public string heroName = "Christian";
    public ArmySlot[] slots = new ArmySlot[7];
}