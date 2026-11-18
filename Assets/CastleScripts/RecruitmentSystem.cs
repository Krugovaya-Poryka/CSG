using System.Collections.Generic;
using UnityEngine;

public class RecruitUnitItem
{
    public string Name { get; set; }
    public int AvailableCount { get; set; }
    public int Price { get; set; }

    public RecruitUnitItem(string name, int availableCount, int price)
    {
        Name = name;
        AvailableCount = availableCount;
        Price = price;
    }
}

public class RecruitmentSystem
{
    // --- ДАНІ (Поля класу) ---
    public int Gold { get; private set; }       
    public bool IsHeroHired { get; private set; }     

    private List<RecruitUnitItem> availableTroops = new List<RecruitUnitItem>();

    private int heroPrice = 1000; 

    // --- КОНСТРУКТОР ---
    public RecruitmentSystem(int startGold)
    {
        Gold = startGold;
        IsHeroHired = false;

        InitializeTroops();

        Debug.Log($"[Recruitment] Система найму готова. Золото: {Gold}. Доступно типів військ у списку: {availableTroops.Count}.");
    }

    private void InitializeTroops()
    {
        availableTroops.Add(new RecruitUnitItem("Мечник", 10, 100));

        availableTroops.Add(new RecruitUnitItem("Лучник", 5, 150));
    }

    // --- МЕТОД ВИВЕДЕННЯ КАТАЛОГУ ВІЙСЬК ---
    public void ShowTroopsCatalog()
    {
        Debug.Log("--- КАТАЛОГ ДОСТУПНИХ ВІЙСЬК (List<RecruitUnitItem>) ---");
        for (int i = 0; i < availableTroops.Count; i++)
        {
            RecruitUnitItem troop = availableTroops[i];
            Debug.Log($"[{i}] {troop.Name} — Ціна: {troop.Price} золота | Доступно для найму: {troop.AvailableCount} шт.");
        }
    }

    // --- МЕТОДИ НАЙМУ ЮНІТІВ ЗА ІНДЕКСОМ У КОЛЕКЦІЇ ---
    public void HireTroops(int troopIndex, int count)
    {
        // Перевірка, чи існує такий юніт у списку
        if (troopIndex < 0 || troopIndex >= availableTroops.Count)
        {
            Debug.Log("[Recruitment] Помилка: Такого типу військ не існує в каталозі!");
            return;
        }

        RecruitUnitItem troop = availableTroops[troopIndex];
        int totalCost = count * troop.Price;

        if (count > troop.AvailableCount)
        {
            Debug.Log($"[Recruitment] Помилка: Не можна найняти {count} од. '{troop.Name}', доступно лише {troop.AvailableCount}!");
            return;
        }

        if (Gold < totalCost)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує золота для найму '{troop.Name}'! Потрібно {totalCost}, а у вас {Gold}.");
            return;
        }

        Gold -= totalCost;
        troop.AvailableCount -= count;
        Debug.Log($"[Recruitment] УСПІХ! Найнято {count} од. '{troop.Name}' за {totalCost} золота! Залишилося золота: {Gold}, залишилося в замку: {troop.AvailableCount}.");
    }

    // --- МЕТОД НАЙМУ ГЕРОЯ ---
    public void HireHero(string heroName)
    {
        if (IsHeroHired)
        {
            Debug.Log("[Recruitment] Помилка: Герой уже найнятий для цього замку!");
            return;
        }

        if (Gold < heroPrice)
        {
            Debug.Log($"[Recruitment] Помилка: Бракує {heroPrice - Gold} золота для найму героя {heroName}!");
            return;
        }

        Gold -= heroPrice;
        IsHeroHired = true;
        Debug.Log($"[Recruitment] УРА! Герой '{heroName}' успішно найнятий за {heroPrice} золота! Залишок золота: {Gold}.");
    }

    public void CloseRecruitment()
    {
        Debug.Log("[Recruitment] Меню найму закрито, ресурси очищено.");
    }
}