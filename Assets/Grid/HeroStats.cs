using UnityEngine;

[System.Serializable]
public class HeroStats
{
    public int attack = 2;
    public int defense = 1;
    public int power = 1;
    public int knowledge = 1;
    public int mana = 10;
    public int MaxMana => Mathf.Max(0, knowledge * 10);
}
