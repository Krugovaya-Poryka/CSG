using System.Collections.Generic;
using UnityEngine;

public class BuildingItem
{
    public string Name { get; set; }
    public int Level { get; set; }

    public BuildingItem(string name, int level)
    {
        Name = name;
        Level = level;
    }
}

public class BuildingUpgradeSystem
{
    public int Gold { get; private set; }

    private List<BuildingItem> buildings = new List<BuildingItem>();

    private int baseUpgradeCost = 200;

    // --- КОНСТРУКТОР ---
    public BuildingUpgradeSystem(int startGold)
    {
        Gold = startGold;
        InitializeBuildings();

        Debug.Log($"[BuildingSystem] Система управління будівлями ініційована. Доступно золота: {Gold}. Всього будівель: {buildings.Count}.");
    }

    // Ініціалізація списку будівель
    private void InitializeBuildings()
    {
        buildings.Add(new BuildingItem("Цитадель", 1));
        buildings.Add(new BuildingItem("Казарми", 1));
        buildings.Add(new BuildingItem("Таверна", 1));
        buildings.Add(new BuildingItem("Кузня", 1));
        buildings.Add(new BuildingItem("Магічна вежа", 1));
        buildings.Add(new BuildingItem("Склад ресурсів", 1));
        buildings.Add(new BuildingItem("Стіни замку", 1));
    }

    // --- МЕТОД ПОКАЗУ СТАТУСУ ВСІХ БУДІВЕЛЬ ---
    public void ShowAllBuildings()
    {
        Debug.Log("--- ПОТОЧНИЙ СТАН БУДІВЕЛЬ ЗАМКУ (List<BuildingItem>) ---");
        for (int i = 0; i < buildings.Count; i++)
        {
            BuildingItem building = buildings[i];
            Debug.Log($"Будівля [{i + 1}] '{building.Name}': Рівень {building.Level}");
        }
    }

    // --- МЕТОД ПОКРАЩЕННЯ КОНКРЕТНОЇ БУДІВЛІ ЗА ІНДЕКСОМ ---
    public void UpgradeBuilding(int index)
    {
        // Перевірка, чи існує така будівля у списку
        if (index < 0 || index >= buildings.Count)
        {
            Debug.Log("[BuildingSystem] Помилка: Немає такої будівлі з цим номером!");
            return;
        }

        BuildingItem building = buildings[index];
        int currentLevel = building.Level;
        int cost = baseUpgradeCost * currentLevel;

        if (Gold < cost)
        {
            Debug.Log($"[BuildingSystem] Помилка: Бракує золота для покращення '{building.Name}' (Рівень {currentLevel} -> {currentLevel + 1})! Потрібно {cost}, а у вас {Gold}.");
            return;
        }

        Gold -= cost;
        building.Level++;
        Debug.Log($"[BuildingSystem] УСПІХ! Будівлю '{building.Name}' покращено до {building.Level} рівня! Витрачено {cost} золота. Залишок золота: {Gold}.");
    }

    // --- ЗАКРИТТЯ / ОЧИЩЕННЯ ---
    public void CloseSystem()
    {
        Debug.Log("[BuildingSystem] Меню управління будівлями закрито, ресурси звільнено.");
    }
}