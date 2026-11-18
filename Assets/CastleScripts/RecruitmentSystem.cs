using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// --- БАЗОВИЙ АБСТРАКТНИЙ КЛАС ---
public abstract class BaseRecruit
{
    public string Name { get; set; }
    public int Price { get; set; }

    protected BaseRecruit(string name, int price)
    {
        Name = name;
        Price = price;
    }

    // Віртуальний поліморфний метод
    public virtual void RecruitAction()
    {
        Debug.Log($"[Recruit] Дія базового рекрута: {Name}");
    }
}

// --- ПОХІДНИЙ КЛАС 1: Звичайний загін військ (має кількість) ---
public class TroopUnit : BaseRecruit
{
    public int AvailableCount { get; set; }

    public TroopUnit(string name, int price, int availableCount)
        : base(name, price)
    {
        AvailableCount = availableCount;
    }

    public override void RecruitAction()
    {
        Debug.Log($"[TroopUnit] Загін '{Name}' приєднується до армії! Доступно в казармах: {AvailableCount} шт.");
    }
}

// --- ПОХІДНИЙ КЛАС 2: Унікальний Герой (наймається в єдиному екземплярі) ---
public class HeroUnit : BaseRecruit
{
    public bool IsHired { get; set; }
    public string SpecialAbility { get; set; }

    public HeroUnit(string name, int price, string specialAbility)
        : base(name, price)
    {
        IsHired = false;
        SpecialAbility = specialAbility;
    }

    public override void RecruitAction()
    {
        Debug.Log($"[HeroUnit] Герой '{Name}' готовий вести війська в бій! Унікальна здібність: {SpecialAbility}");
    }
}

// --- ОСНОВНА СИСТЕМА НАЙМУ ---
public class RecruitmentSystem
{
    public int Gold { get; private set; }

    // Поліморфна колекція List<BaseRecruit>, що містить і звичайні війська, і героїв
    private List<BaseRecruit> availableRecruits = new List<BaseRecruit>();

    public RecruitmentSystem(int startGold)
    {
        Gold = startGold;
        InitializeRecruits();
    }

    private void InitializeRecruits()
    {
        availableRecruits.Add(new TroopUnit("Мечник", 100, 10));
        availableRecruits.Add(new TroopUnit("Лучник", 150, 5));
        availableRecruits.Add(new HeroUnit("Артур", 1000, "Лютість воїнів (+20% атаки)"));
        availableRecruits.Add(new HeroUnit("Моргана", 1200, "Магічний щит (+30% захисту)"));
    }

    public void ShowCatalog()
    {
        Debug.Log("--- КАТАЛОГ НАЙМУ (Ієрархія ЛР№3) ---");
        for (int i = 0; i < availableRecruits.Count; i++)
        {
            BaseRecruit r = availableRecruits[i];
            if (r is TroopUnit troop)
            {
                Debug.Log($"[{i}] Загін: {troop.Name} — Ціна: {troop.Price} золота | Доступно: {troop.AvailableCount} шт.");
            }
            else if (r is HeroUnit hero)
            {
                Debug.Log($"[{i}] Герой: {hero.Name} — Ціна: {hero.Price} золота | Найнятий: {hero.IsHired} | Здібність: {hero.SpecialAbility}");
            }
        }
    }

    // Демонстрація поліморфізму в дії
    public void TestRecruitPolymorphism(int index)
    {
        if (index >= 0 && index < availableRecruits.Count)
        {
            availableRecruits[index].RecruitAction();
        }
    }

    // Найм звичайного загону за індексом
    public void HireTroops(int index, int count)
    {
        if (index < 0 || index >= availableRecruits.Count)
        {
            Debug.Log("[Recruitment] Помилка: Такого елемента не існує!");
            return;
        }

        if (availableRecruits[index] is not TroopUnit troop)
        {
            Debug.Log("[Recruitment] Помилка: Це не загін військ, а герой!");
            return;
        }

        int totalCost = count * troop.Price;

        if (count > troop.AvailableCount)
        {
            Debug.Log($"[Recruitment] Помилка: Не можна найняти {count} од. '{troop.Name}', доступно лише {troop.AvailableCount}!");
            return;
        }

        if (Gold < totalCost)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує золота! Потрібно {totalCost}, а у вас {Gold}.");
            return;
        }

        Gold -= totalCost;
        troop.AvailableCount -= count;
        Debug.Log($"[Recruitment] УСПІХ! Найнято {count} од. '{troop.Name}' за {totalCost} золота! Залишок золота: {Gold}.");
    }

    // Найм героя за індексом
    public void HireHero(int index)
    {
        if (index < 0 || index >= availableRecruits.Count)
        {
            Debug.Log("[Recruitment] Помилка: Такого елемента не існує!");
            return;
        }

        if (availableRecruits[index] is not HeroUnit hero)
        {
            Debug.Log("[Recruitment] Помилка: Це не герой, а звичайний загін!");
            return;
        }

        if (hero.IsHired)
        {
            Debug.Log($"[Recruitment] Помилка: Герой '{hero.Name}' уже найнятий!");
            return;
        }

        if (Gold < hero.Price)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує золота для найму героя '{hero.Name}'! Потрібно {hero.Price}, а у вас {Gold}.");
            return;
        }

        Gold -= hero.Price;
        hero.IsHired = true;
        Debug.Log($"[Recruitment] УРА! Герой '{hero.Name}' успішно найнятий за {hero.Price} золота! Залишок золота: {Gold}.");
    }

    // --- ФУНКЦІЯ ПОШУКУ ---
    public void SearchRecruitsByMaxPrice(int maxPrice)
    {
        Debug.Log($"--- ПОШУК ЮНІТІВ (дешевші або рівні {maxPrice} золота) ---");
        var results = availableRecruits.Where(r => r.Price <= maxPrice).ToList();

        foreach (var r in results)
        {
            Debug.Log($"Знайдено: {r.Name} — Ціна: {r.Price}");
        }
    }

    // --- ФУНКЦІЯ СОРТУВАННЯ ---
    public void SortRecruitsByPrice()
    {
        availableRecruits = availableRecruits.OrderBy(r => r.Price).ToList();
        Debug.Log("[Recruitment] Каталог найму відсортовано за ціною!");
        ShowCatalog();
    }

    // --- ФУНКЦІЯ ЗБЕРЕЖЕННЯ ДАНИХ ---
    public string SaveRecruitmentState()
    {
        string stateData = "";
        foreach (var r in availableRecruits)
        {
            if (r is TroopUnit troop)
            {
                stateData += $"{troop.Name}:{troop.AvailableCount};";
            }
            else if (r is HeroUnit hero)
            {
                stateData += $"{hero.Name}:{hero.IsHired};";
            }
        }
        Debug.Log($"[Recruitment] Стан системи найму успішно збережено: {stateData}");
        return stateData;
    }

    public void CloseRecruitment()
    {
        Debug.Log("[Recruitment] Меню найму закрито, ресурси очищено.");
    }
}