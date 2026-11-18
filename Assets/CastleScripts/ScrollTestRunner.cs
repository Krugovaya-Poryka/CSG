using UnityEngine;

public class ScrollTestRunner : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("=== ТЕСТУВАННЯ СИСТЕМИ ПОКУПКИ СВИТКІВ (20 шт.) ===");

        // 1. Створюємо магазин свитків із 1000 золота
        ScrollShopSystem scrollShop = new ScrollShopSystem(1000);

        // 2. Виводимо весь каталог із 20 свитків
        scrollShop.ShowCatalog();

        // 3. Купуємо кілька свитків за їх індексами
        scrollShop.BuyScroll(0);   // "Свиток Вогняна куля" (ціна 150)
        scrollShop.BuyScroll(4);   // "Свиток Щит магії" (ціна 180)
        scrollShop.BuyScroll(17);  // "Свиток Воскресіння" (ціна 500)

        // 4. Пробуємо купити дорогий свиток, на який вже не вистачить золота (наприклад, індекс 7 — "Метеоритний дощ", ціна 400)
        scrollShop.BuyScroll(7);

        // 5. Закриваємо магазин та очищуємо посилання
        scrollShop.CloseShop();
        scrollShop = null;
    }
}