using UnityEngine;

// Клас, що відповідає за систему покращення 7 будівель у замку
public class BuildingUpgradeSystem
{
    // --- ДАНІ (Поля класу) ---
    public int Gold { get; private set; }              // Золото гравця
    
    // Масиви для зберігання назв будівель та їх рівнів (всього 7 будівель)
    private string[] buildingNames = new string[7] 
    { 
        "Цитадель", "Казарми", "Таверна", "Кузня", "Магічна вежа", "Склад ресурсів", "Стіни замку" 
    };
    
    private int[] buildingLevels = new int[7] { 1, 1, 1, 1, 1, 1, 1 }; // Початковий рівень усіх будівель — 1
    
    private int baseUpgradeCost = 200; // Базова вартість покращення

    // --- КОНСТРУКТОР ---
    public BuildingUpgradeSystem(int startGold)
    {
        Gold = startGold;
        Debug.Log($"[BuildingSystem] Система управління будівлями ініційована. Доступно золота: {Gold}. Всього будівель: 7.");
    }

    // --- МЕТОД ПОКАЗУ СТАТУСУ ВСІХ БУДІВЕЛЬ ---
    public void ShowAllBuildings()
    {
        Debug.Log("--- ПОТОЧНИЙ СТАН БУДІВЕЛЬ ЗАМКУ ---");
        for (int i = 0; i < buildingNames.Length; i++)
        {
            Debug.Log($"Будівля [{i + 1}] '{buildingNames[i]}': Рівень {buildingLevels[i]}");
        }
    }

    // --- МЕТОД ПОКРАЩЕННЯ КОНКРЕТНОЇ БУДІВЛІ ЗА ІНДЕКСОМ (від 0 до 6) ---
    public void UpgradeBuilding(int index)
    {
        // Перевірка, чи існує така будівля
        if (index < 0 || index >= buildingNames.Length)
        {
            Debug.Log("[BuildingSystem] Помилка: Немає такої будівлі з цим номером!");
            return;
        }

        string name = buildingNames[index];
        int currentLevel = buildingLevels[index];
        int cost = baseUpgradeCost * currentLevel; // Чим вищий рівень, тим дорожче покращення

        // Перевірка золота
        if (Gold < cost)
        {
            Debug.Log($"[BuildingSystem] Помилка: Бракує золота для покращення '{name}' (Рівень {currentLevel} -> {currentLevel + 1})! Потрібно {cost}, а у вас {Gold}.");
            return;
        }

        // Виконання покращення
        Gold -= cost;
        buildingLevels[index]++;
        Debug.Log($"[BuildingSystem] УСПІХ! Будівлю '{name}' покращено до {buildingLevels[index]} рівня! Витрачено {cost} золота. Залишок золота: {Gold}.");
    }

    // --- ЗАКРИТТЯ / ОЧИЩЕННЯ ---
    public void CloseSystem()
    {
        Debug.Log("[BuildingSystem] Меню управління будівлями закрито, ресурси звільнено.");
    }
}