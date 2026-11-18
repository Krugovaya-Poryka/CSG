using UnityEngine;

public class BuildingTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ СИСТЕМИ ПОКРАЩЕННЯ 7 БУДІВЕЛЬ ЗАМКУ ===");

        // 1. Створюємо систему з 1000 золота
        BuildingUpgradeSystem buildingSystem = new BuildingUpgradeSystem(1000);

        // 2. Показуємо початковий стан усіх 7 будівель
        buildingSystem.ShowAllBuildings();

        // 3. Пробуємо покращити першу будівлю ("Цитадель", індекс 0)
        buildingSystem.UpgradeBuilding(0); // Коштує 200 * 1 = 200 золота

        // 4. Пробуємо покращити третю будівлю ("Таверна", індекс 2)
        buildingSystem.UpgradeBuilding(2); // Коштує 200 * 1 = 200 золота

        // 5. Пробуємо ще раз покращити "Цитадель" (тепер вона 2 рівня, коштуватиме 200 * 2 = 400 золота)
        buildingSystem.UpgradeBuilding(0);

        // 6. Показуємо оновлений стан будівель
        buildingSystem.ShowAllBuildings();

        // 7. Закриваємо систему та очищуємо пам'ять
        buildingSystem.CloseSystem();
        buildingSystem = null;
    }
}