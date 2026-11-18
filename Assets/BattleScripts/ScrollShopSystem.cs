using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScrollShopSystem
{
    public int Gold { get; private set; }

    private List<MagicalScroll> availableScrolls = new List<MagicalScroll>();

    public ScrollShopSystem(int startGold)
    {
        Gold = startGold;
        InitializeCatalog();
    }

    private void InitializeCatalog()
    {
        availableScrolls.Add(new CombatScroll("Вогняна куля", 150, 3, 75));
        availableScrolls.Add(new SupportScroll("Зцілення", 100, 3, 50));
        availableScrolls.Add(new CombatScroll("Метеоритний дощ", 400, 2, 200));
        availableScrolls.Add(new SupportScroll("Щит магії", 180, 4, 30));
    }

    public void ShowCatalog()
    {
        Debug.Log("--- КАТАЛОГ СВИТКІВ (Поліморфна ієрархія) ---");
        for (int i = 0; i < availableScrolls.Count; i++)
        {
            MagicalScroll scroll = availableScrolls[i];
            Debug.Log($"[{i}] {scroll.Name} — Ціна: {scroll.Price} | Залишок: {scroll.Stock} (Тип: {scroll.GetType().Name})");
        }
    }

    public void TestScrollPolymorphism(int index)
    {
        if (index >= 0 && index < availableScrolls.Count)
        {
            availableScrolls[index].UseEffect();
        }
    }

    // --- ФУНКЦІЯ ПОШУКУ ---
    // Пошук свитків, які дешевші за вказану ціну
    public void SearchScrollsByMaxPrice(int maxPrice)
    {
        Debug.Log($"--- ПОШУК СВИТКІВ (дешевші за {maxPrice} золота) ---");
        var results = availableScrolls.Where(s => s.Price <= maxPrice).ToList();

        foreach (var scroll in results)
        {
            Debug.Log($"Знайдено: {scroll.Name} — Ціна: {scroll.Price}");
        }
    }

    // --- ФУНКЦІЯ СОРТУВАННЯ ---
    // Сортування свитків за ціною (від найдешевших до найдорожчих)
    public void SortScrollsByPrice()
    {
        availableScrolls = availableScrolls.OrderBy(s => s.Price).ToList();
        Debug.Log("[ScrollShop] Каталог відсортовано за ціною!");
        ShowCatalog();
    }

    // --- ФУНКЦІЯ ЗБЕРЕЖЕННЯ ТА ВІДНОВЛЕННЯ ---
    // Імітація збереження стану (наприклад, вивід поточних залишків у рядок)
    public string SaveShopState()
    {
        string stateData = "";
        foreach (var scroll in availableScrolls)
        {
            stateData += $"{scroll.Name}:{scroll.Stock};";
        }
        Debug.Log($"[ScrollShop] Дані магазину збережено: {stateData}");
        return stateData;
    }

    public void BuyScroll(int index)
    {
        if (index < 0 || index >= availableScrolls.Count) return;
        MagicalScroll scroll = availableScrolls[index];

        if (Gold < scroll.Price || scroll.Stock <= 0)
        {
            Debug.Log("[ScrollShop] Помилка покупки (немає золота або товару)!");
            return;
        }

        Gold -= scroll.Price;
        scroll.Stock--;
        Debug.Log($"[ScrollShop] Куплено '{scroll.Name}'. Залишок золота: {Gold}");
    }
}