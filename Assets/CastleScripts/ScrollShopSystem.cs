using System.Collections.Generic;
using UnityEngine;

public class ScrollItem
{
    public string Name { get; set; }
    public int Price { get; set; }
    public int Stock { get; set; }

    public ScrollItem(string name, int price, int stock)
    {
        Name = name;
        Price = price;
        Stock = stock;
    }
}

public class ScrollShopSystem
{
    // --- ДАНІ (Поля класу) ---
    public int Gold { get; private set; } // Золото гравця

    private List<ScrollItem> availableScrolls = new List<ScrollItem>();

    // --- КОНСТРУКТОР ---
    public ScrollShopSystem(int startGold)
    {
        Gold = startGold;
        InitializeCatalog();

        Debug.Log($"[ScrollShop] Магазин свитків і магії відкрито. Золото гравця: {Gold}. Асортимент: {availableScrolls.Count} унікальних свитків.");
    }

    private void InitializeCatalog()
    {
        availableScrolls.Add(new ScrollItem("Свиток Вогняна куля", 150, 3));
        availableScrolls.Add(new ScrollItem("Свиток Крижана стріла", 120, 3));
        availableScrolls.Add(new ScrollItem("Свиток Блискавка", 200, 3));
        availableScrolls.Add(new ScrollItem("Свиток Зцілення", 100, 3));
        availableScrolls.Add(new ScrollItem("Свиток Щит магії", 180, 3));
        availableScrolls.Add(new ScrollItem("Свиток Телепортація", 250, 3));
        availableScrolls.Add(new ScrollItem("Свиток Отруйна хмара", 160, 3));
        availableScrolls.Add(new ScrollItem("Свиток Метеоритний дощ", 400, 3));
        availableScrolls.Add(new ScrollItem("Свиток Невидимість", 300, 3));
        availableScrolls.Add(new ScrollItem("Свиток Ланцюгова блискавка", 220, 3));
        availableScrolls.Add(new ScrollItem("Свиток Заклинання сили", 130, 3));
        availableScrolls.Add(new ScrollItem("Свиток Швидкість вітру", 110, 3));
        availableScrolls.Add(new ScrollItem("Свиток Водяний щит", 140, 3));
        availableScrolls.Add(new ScrollItem("Свиток Землетрус", 350, 3));
        availableScrolls.Add(new ScrollItem("Свиток Благословення", 190, 3));
        availableScrolls.Add(new ScrollItem("Свиток Прокляття", 210, 3));
        availableScrolls.Add(new ScrollItem("Свиток Пошук скарбів", 80, 3));
        availableScrolls.Add(new ScrollItem("Свиток Воскресіння", 500, 3));
        availableScrolls.Add(new ScrollItem("Свиток Вогняна стіна", 240, 3));
        availableScrolls.Add(new ScrollItem("Свиток Абсолютний захист", 450, 3));
    }

    // --- МЕТОД ВИВЕДЕННЯ КАТАЛОГУ ВСІХ 20 СВИТКІВ ---
    public void ShowCatalog()
    {
        Debug.Log("--- КАТАЛОГ СВИТКІВ ТА ЗАКЛИНАНЬ (List<ScrollItem>) ---");
        for (int i = 0; i < availableScrolls.Count; i++)
        {
            ScrollItem scroll = availableScrolls[i];
            Debug.Log($"[{i}] {scroll.Name} — Ціна: {scroll.Price} золота | В наявності: {scroll.Stock} шт.");
        }
    }

    // --- МЕТОД ПОКУПКИ СВИТКА ЗА ІНДЕКСОМ (від 0 до 19) ---
    public void BuyScroll(int index)
    {
        if (index < 0 || index >= availableScrolls.Count)
        {
            Debug.Log("[ScrollShop] Помилка: Такого свитка не існує в каталозі!");
            return;
        }

        ScrollItem scroll = availableScrolls[index];

        if (scroll.Stock <= 0)
        {
            Debug.Log($"[ScrollShop] Помилка: Свиток '{scroll.Name}' повністю розкуплено!");
            return;
        }

        if (Gold < scroll.Price)
        {
            Debug.Log($"[ScrollShop] Помилка: Бракує золота для покупки '{scroll.Name}' (Потрібно {scroll.Price}, а у вас {Gold}).");
            return;
        }

        Gold -= scroll.Price;
        scroll.Stock--;
        Debug.Log($"[ScrollShop] УСПІХ! Куплено '{scroll.Name}' за {scroll.Price} золота. Залишок золота: {Gold}. Залишилось в магазині: {scroll.Stock} шт.");
    }

    // --- ЗАКРИТТЯ / ОЧИЩЕННЯ ---
    public void CloseShop()
    {
        Debug.Log("[ScrollShop] Крамницю свитків закрито, пам'ять звільнено.");
    }
}