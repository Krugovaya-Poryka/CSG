using UnityEngine;

public class BuildingTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ ЛР№3: СИСТЕМА БУДІВЕЛЬ (НАСЛІДУВАННЯ ТА ПОЛІМОРФІЗМ) ===");

        // 1. Створюємо систему будівель із 1500 золота
        BuildingUpgradeSystem buildingSystem = new BuildingUpgradeSystem(1500);

        // 2. Виводимо початковий стан
        buildingSystem.ShowAllBuildings();

        // 3. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ
        Debug.Log("\n--- ПЕРЕВІРКА ПОЛІМОРФІЗМУ БУДІВЕЛЬ ---");
        buildingSystem.TestBuildingPolymorphism(0); // Цитадель (EconomicBuilding)
        buildingSystem.TestBuildingPolymorphism(1); // Казарми (MilitaryBuilding)

        // 4. Виконуємо стандартне покращення будівлі
        Debug.Log("\n--- ПОКРАЩЕННЯ БУДІВЛІ ---");
        buildingSystem.UpgradeBuilding(0); // Покращуємо Цитадель

        // 5. ДЕМОНСТРАЦІЯ СОРТУВАННЯ
        Debug.Log("\n--- ПЕРЕВІРКА СОРТУВАННЯ ---");
        buildingSystem.SortBuildingsByLevel();

        // 6. ДЕМОНСТРАЦІЯ ПОШУКУ
        Debug.Log("\n--- ПЕРЕВІРКА ПОШУКУ ---");
        buildingSystem.SearchBuildingsByName("Магічна");

        // 7. ЗБЕРЕЖЕННЯ ДАНИХ
        Debug.Log("\n--- ПЕРЕВІРКА ЗБЕРЕЖЕННЯ СТАНУ ---");
        buildingSystem.SaveBuildingState();

        // 8. Закриття системи
        buildingSystem.CloseSystem();
        buildingSystem = null;
    }
}