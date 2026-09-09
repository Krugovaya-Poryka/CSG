using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitData", menuName = "HOMM3/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Sprite unitIcon;

    [Header("Базові характеристики")]
    public int maxHealth = 10;
    public int attack = 5;
    public int defense = 5;
    public int minDamage = 2;
    public int maxDamage = 4;

    [Header("Переміщення та Ініціатива")]
    public int speed = 6; // Швидкість визначає і дальність ходу на гексах, і чергу в бою!

    [Header("Особливості")]
    public bool isRanged = false;       // Стрілець
    public bool isFlyer = false;        // Літає (ігнорує перешкоди)
    public int maxShots = 0;            // Кількість пострілів
    public int retaliationsCount = 1;   // Кількість контратак за раунд
}