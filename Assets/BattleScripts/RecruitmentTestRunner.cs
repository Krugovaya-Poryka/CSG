using UnityEngine;

public class RecruitmentTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("ТЕСТУВАННЯ СИСТЕМИ НАЙМУ (ЮНІТИ ТА ГЕРОЇ)");

        // 1. Створюємо систему найму з 1500 золота
        RecruitmentSystem recruitment = new RecruitmentSystem(1500);

        // 2. Пробуємо найняти 3 мечників (3 * 100 = 300 золота)
        recruitment.HireSwordsmen(3);

        // 3. Пробуємо найняти героя "Князь" (1000 золота)
        recruitment.HireHero("Артур");

        // 4. Пробуємо найняти ще 5 лучників (5 * 150 = 750) — золота вже не вистачить!
        recruitment.HireArchers(5);

        recruitment.CloseRecruitment();
        recruitment = null;
    }
}