using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// БАЗОВИЙ АБСТРАКТНИЙ КЛАС (Наслідування)
public abstract class MagicalScroll
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }

    protected MagicalScroll(string name, int price, int stock)
    {
        Name = name;
        Price = price;
        Stock = stock;
    }

    // Віртуальний поліморфний метод (кожен дочірній клас перевизначить його по-своєму)
    public virtual void UseEffect()
    {
        Debug.Log($"[Scroll] Використано базовий магічний свиток: {Name}");
    }
}

// ПОХІДНИЙ КЛАС 1: Бойовий свиток (наприклад, атака)
public class CombatScroll : MagicalScroll
{
    public int Damage { get; set; }

    public CombatScroll(string name, int price, int stock, int damage)
        : base(name, price, stock)
    {
        Damage = damage;
    }

    // Поліморфне перевизначення методу
    public override void UseEffect()
    {
        Debug.Log($"[CombatScroll] БУХ! Свиток '{Name}' завдає {Damage} одиниць магічної шкоди ворогу!");
    }
}

// ПОХІДНИЙ КЛАС 2: Свиток підтримки (наприклад, зцілення чи захист)
public class SupportScroll : MagicalScroll
{
    public int PowerBonus { get; set; }

    public SupportScroll(string name, int price, int stock, int powerBonus)
        : base(name, price, stock)
    {
        PowerBonus = powerBonus;
    }

    // Поліморфне перевизначення методу
    public override void UseEffect()
    {
        Debug.Log($"[SupportScroll] Сяйво! Свиток '{Name}' відновлює/збільшує параметр на {PowerBonus} од.!");
    }
}