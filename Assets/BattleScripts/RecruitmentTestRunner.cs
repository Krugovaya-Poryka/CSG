using UnityEngine;

public class RecruitmentTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ ЛР№3: СИСТЕМА НАЙМУ (НАСЛІДУВАННЯ ТА ПОЛІМОРФІЗМ) ===");

        // 1. Створюємо систему найму із 2500 золота
        RecruitmentSystem recruitment = new RecruitmentSystem(2500);

        // 2. Виводимо початковий каталог
        recruitment.ShowCatalog();

        // 3. ДЕМОНСТРАЦІЯ ПОЛІМОРФІЗМУ
        Debug.Log("\n--- ПЕРЕВІРКА ПОЛІМОРФІЗМУ НАЙМУ ---");
        recruitment.TestRecruitPolymorphism(0); // Мечник (TroopUnit)
        recruitment.TestRecruitPolymorphism(2); // Герой Артур (HeroUnit)

        // 4. ДЕМОНСТРАЦІЯ НАЙМУ
        Debug.Log("\n--- ВИКОНАННЯ НАЙМУ ---");
        recruitment.HireTroops(0, 3); // Найм 3 мечників (індекс 0)
        recruitment.HireHero(2);       // Найм героя Артура (індекс 2)

        // 5. ДЕМОНСТРАЦІЯ СОРТУВАННЯ
        Debug.Log("\n--- ПЕРЕВІРКА СОРТУВАННЯ ЗА ЦІНОЮ ---");
        recruitment.SortRecruitsByPrice();

        // 6. ДЕМОНСТРАЦІЯ ПОШУКУ
        Debug.Log("\n--- ПЕРЕВІРКА ПОШУКУ (до 200 золота) ---");
        recruitment.SearchRecruitsByMaxPrice(200);

        // 7. ЗБЕРЕЖЕННЯ ДАНИХ
        Debug.Log("\n--- ПЕРЕВІРКА ЗБЕРЕЖЕННЯ СТАНУ ---");
        recruitment.SaveRecruitmentState();

        // 8. Закриття системи
        recruitment.CloseRecruitment();
        recruitment = null;
    }
}