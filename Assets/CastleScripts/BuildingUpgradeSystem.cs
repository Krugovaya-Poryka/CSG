using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// --- БАЗОВИЙ АБСТРАКТНИЙ КЛАС ---
public abstract class BaseBuilding
{
    public string Name { get; set; }
    public int Level { get; set; }
    public int BaseCost { get; set; }

    protected BaseBuilding(string name, int level, int baseCost)
    {
        Name = name;
        Level = level;
        BaseCost = baseCost;
    }

    // Віртуальний поліморфний метод (кожна будівля виконує свою унікальну дію)
    public virtual void ActivateBuilding()
    {
        Debug.Log($"[Building] Дія базової будівлі: {Name}");
    }
}

// --- ПОХІДНИЙ КЛАС 1: Економічна будівля ---
public class EconomicBuilding : BaseBuilding
{
    public int GoldProduction { get; set; }

    public EconomicBuilding(string name, int level, int baseCost, int goldProduction)
        : base(name, level, baseCost)
    {
        GoldProduction = goldProduction;
    }

    public override void ActivateBuilding()
    {
        Debug.Log($"[EconomicBuilding] Економічна будівля '{Name}' генерує {GoldProduction * Level} золота в хвилину!");
    }
}

// --- ПОХІДНИЙ КЛАС 2: Військова будівля ---
public class MilitaryBuilding : BaseBuilding
{
    public int DefenseBonus { get; set; }

    public MilitaryBuilding(string name, int level, int baseCost, int defenseBonus)
        : base(name, level, baseCost)
    {
        DefenseBonus = defenseBonus;
    }

    public override void ActivateBuilding()
    {
        Debug.Log($"[MilitaryBuilding] Військова будівля '{Name}' зміцнює оборону замку: +{DefenseBonus * Level} до захисту!");
    }
}

// --- ОСНОВНА СИСТЕМА УПРАВЛІННЯ БУДІВЛЯМИ ---
public class BuildingUpgradeSystem
{
    public int Gold { get; private set; }

    // Поліморфна колекція List<BaseBuilding>, що містить об'єкти різних підкласів
    private List<BaseBuilding> buildings = new List<BaseBuilding>();

    public BuildingUpgradeSystem(int startGold)
    {
        Gold = startGold;
        InitializeBuildings();
    }

    private void InitializeBuildings()
    {
        // Додаємо і економічні, і військові будівлі в єдиний список завдяки наслідуванню
        buildings.Add(new EconomicBuilding("Цитадель", 1, 200, 50));
        buildings.Add(new MilitaryBuilding("Казарми", 1, 150, 20));
        buildings.Add(new EconomicBuilding("Таверна", 1, 100, 35));
        buildings.Add(new MilitaryBuilding("Кузня", 1, 180, 25));
        buildings.Add(new EconomicBuilding("Магічна вежа", 1, 300, 80));
        buildings.Add(new EconomicBuilding("Склад ресурсів", 1, 120, 40));
        buildings.Add(new MilitaryBuilding("Стіни замку", 1, 250, 50));
    }

    public void ShowAllBuildings()
    {
        Debug.Log("--- ПОТОЧНИЙ СТАН БУДІВЕЛЬ (Ієрархія ЛР№3) ---");
        for (int i = 0; i < buildings.Count; i++)
        {
            BaseBuilding b = buildings[i];
            Debug.Log($"Будівля [{i}] '{b.Name}' — Рівень: {b.Level} | Вартість апгрейду: {b.BaseCost * b.Level} | Тип: {b.GetType().Name}");
        }
    }

    // Демонстрація поліморфізму в дії
    public void TestBuildingPolymorphism(int index)
    {
        if (index >= 0 && index < buildings.Count)
        {
            buildings[index].ActivateBuilding();
        }
    }

    public void UpgradeBuilding(int index)
    {
        if (index < 0 || index >= buildings.Count)
        {
            Debug.Log("[BuildingSystem] Помилка: Немає такої будівлі з цим номером!");
            return;
        }

        BaseBuilding building = buildings[index];
        int cost = building.BaseCost * building.Level;

        if (Gold < cost)
        {
            Debug.Log($"[BuildingSystem] Помилка: Бракує золота для покращення '{building.Name}' (Потрібно {cost}, а у вас {Gold}).");
            return;
        }

        Gold -= cost;
        building.Level++;
        Debug.Log($"[BuildingSystem] УСПІХ! Будівлю '{building.Name}' покращено до {building.Level} рівня! Витрачено {cost} золота. Залишок: {Gold}.");
    }

    // --- ФУНКЦІЯ ПОШУКУ ---
    public void SearchBuildingsByName(string keyword)
    {
        Debug.Log($"--- ПОШУК БУДІВЕЛЬ ЗА НАЗВОЮ ('{keyword}') ---");
        var results = buildings.Where(b => b.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        foreach (var b in results)
        {
            Debug.Log($"Знайдено: {b.Name} (Рівень: {b.Level})");
        }
    }

    // --- ФУНКЦІЯ СОРТУВАННЯ ---
    public void SortBuildingsByLevel()
    {
        buildings = buildings.OrderByDescending(b => b.Level).ThenBy(b => b.Name).ToList();
        Debug.Log("[BuildingSystem] Будівлі відсортовано за рівнем (від найвищого)!");
        ShowAllBuildings();
    }

    // --- ФУНКЦІЯ ЗБЕРЕЖЕННЯ ДАНИХ ---
    public string SaveBuildingState()
    {
        string stateData = "";
        foreach (var b in buildings)
        {
            stateData += $"{b.Name}:{b.Level};";
        }
        Debug.Log($"[BuildingSystem] Стан будівель успішно збережено: {stateData}");
        return stateData;
    }

    public void CloseSystem()
    {
        Debug.Log("[BuildingSystem] Меню управління будівлями закрито, ресурси звільнено.");
    }
}