using UnityEngine;

public class ScrollTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ ЛР№3: НАСЛІДУВАННЯ ТА ПОЛІМОРФІЗМ ===");

        // 1. Створюємо магазин із поліморфною колекцією List<MagicalScroll> та 1000 золота
        ScrollShopSystem scrollShop = new ScrollShopSystem(1000);

        // 2. Виводимо початковий каталог (з урахуванням спадкування класів)
        scrollShop.ShowCatalog();

        // 3. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ
        // Викликаємо метод UseEffect(), який по-різному працює для CombatScroll та SupportScroll
        Debug.Log("\n--- ПЕРЕВІРКА ПОЛІМОРФІЗМУ ---");
        scrollShop.TestScrollPolymorphism(0); // Бойовий свиток ("Вогняна куля")
        scrollShop.TestScrollPolymorphism(1); // Свиток підтримки ("Зцілення")

        // 4. ДЕМОНСТРАЦІЯ СОРТУВАННЯ
        Debug.Log("\n--- ПЕРЕВІРКА СОРТУВАННЯ ЗА ЦІНОЮ ---");
        scrollShop.SortScrollsByPrice();

        // 5. ДЕМОНСТРАЦІЯ ПОШУКУ
        Debug.Log("\n--- ПЕРЕВІРКА ПОШУКУ (дешевші за 200 золота) ---");
        scrollShop.SearchScrollsByMaxPrice(200);

        // 6. ЗБЕРЕЖЕННЯ ДАНИХ
        Debug.Log("\n--- ПЕРЕВІРКА ЗБЕРЕЖЕННЯ СТАНУ ---");
        scrollShop.SaveShopState();

        // 7. Стандартна покупка та завершення
        scrollShop.BuyScroll(0);
        scrollShop = null;
    }
}