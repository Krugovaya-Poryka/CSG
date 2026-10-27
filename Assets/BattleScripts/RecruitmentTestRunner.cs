using UnityEngine;

public class RecruitmentTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ СИСТЕМИ НАЙМУ ТА ГЕРОЇВ  ===");

        // 1. Створюємо систему найму з 1500 золота
        RecruitmentSystem recruitment = new RecruitmentSystem(1500);

        // 2. Показуємо початковий каталог доступних військ у списку List<T>
        recruitment.ShowTroopsCatalog();

        // 3. Пробуємо найняти 3 мечників (індекс 0 у списку: 3 * 100 = 300 золота)
        recruitment.HireTroops(0, 3);

        // 4. Пробуємо найняти героя "Артур" (1000 золота)
        recruitment.HireHero("Артур");

        // 5. Пробуємо найняти 5 лучників (індекс 1 у списку: 5 * 150 = 750 золота)
        // На цей момент у гравця залишилося 200 золота, тому система виведе помилку про недостачу коштів!
        recruitment.HireTroops(1, 5);

        // 6. Закриваємо систему найму та очищуємо пам'ять
        recruitment.CloseRecruitment();
        recruitment = null;
    }
}