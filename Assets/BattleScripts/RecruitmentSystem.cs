using UnityEngine;

// Клас, що відповідає за всю логіку найму військ і героїв у замку
public class RecruitmentSystem
{
    // --- ДАНІ (Поля класу) ---
    public int Gold { get; private set; }              // Золото гравця для покупок
    public int AvailableSwordsmen { get; private set; } // Доступно мечників
    public int AvailableArchers { get; private set; }   // Доступно лучників
    public bool IsHeroHired { get; private set; }       // Чи найнятий уже головний герой

    // Ціни (константи)
    private const int SwordsmanPrice = 100;
    private const int ArcherPrice = 150;
    private const int HeroPrice = 1000;

    // --- КОНСТРУКТОР (Ініціалізація найму при вході в таверну/казарму) ---
    public RecruitmentSystem(int startGold)
    {
        Gold = startGold;
        AvailableSwordsmen = 10; // На початку доступно 10 мечників
        AvailableArchers = 5;     // На початку доступно 5 лучників
        IsHeroHired = false;      // Герой ще не найнятий

        Debug.Log($"[Recruitment] Система найму готова. Золото: {Gold}. Доступно: Мечників - {AvailableSwordsmen}, Лучників - {AvailableArchers}.");
    }

    // --- МЕТОДИ НАЙМУ ЮНІТІВ ---

    // Найм Мечників
    public void HireSwordsmen(int count)
    {
        int totalCost = count * SwordsmanPrice;

        if (count > AvailableSwordsmen)
        {
            Debug.Log($"[Recruitment] Помилка: Не можна найняти {count} мечників, доступно лише {AvailableSwordsmen}!");
            return;
        }

        if (Gold < totalCost)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує золота! Потрібно {totalCost}, а у вас {Gold}.");
            return;
        }

        Gold -= totalCost;
        AvailableSwordsmen -= count;
        Debug.Log($"[Recruitment] Найнято {count} мечників за {totalCost} золота! Залишилося золота: {Gold}, залишилося мечників: {AvailableSwordsmen}.");
    }

    // Найм Лучників
    public void HireArchers(int count)
    {
        int totalCost = count * ArcherPrice;

        if (count > AvailableArchers)
        {
            Debug.Log($"[Recruitment] Помилка: Недостатньо доступних лучників!");
            return;
        }

        if (Gold < totalCost)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує золота для покупки лучників!");
            return;
        }

        Gold -= totalCost;
        AvailableArchers -= count;
        Debug.Log($"[Recruitment] Найнято {count} лучників за {totalCost} золота! Залишилося золота: {Gold}.");
    }

    // --- МЕТОД НАЙМУ ГЕРОЯ ---

    public void HireHero(string heroName)
    {
        if (IsHeroHired)
        {
            Debug.Log("[Recruitment] Помилка: Герой уже найнятий для цього замку!");
            return;
        }

        if (Gold < HeroPrice)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує {HeroPrice - Gold} золота для найму героя {heroName}!");
            return;
        }

        Gold -= HeroPrice;
        IsHeroHired = true;
        Debug.Log($"[Recruitment] УРА! Герой '{heroName}' успішно найнятий за {HeroPrice} золота! Залишок золота: {Gold}.");
    }

    public void CloseRecruitment()
    {
        Debug.Log("[Recruitment] Меню найму закрито, ресурси очищено.");
    }
}