using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public UnitData data; // Посилання на конфіг з базовими статами

    [Header("Поточний стан стеку")]
    public int stackSize = 10;          // Кількість юнітів у загоні (напр. 10 Пікінерів)
    public int currentTopUnitHP;        // Здоров'я верхнього (пораненого) юніта
    public Vector2Int hexCoords;        // Позиція на гексагональній сітці (q, r)
    public int teamId;                  // 0 = Нападаючий, 1 = Захисник

    [Header("Стан у раунді")]
    public int remainingRetaliations;   // Скільки контратак залишилося в цьому раунді
    public bool hasTakenTurn;           // Чи ходив юніт у цьому раунді

    public void Init(UnitData unitData, int count, int team)
    {
        data = unitData;
        stackSize = count;
        currentTopUnitHP = data.maxHealth;
        teamId = team;
        ResetForNewRound();
    }

    public void ResetForNewRound()
    {
        remainingRetaliations = data.retaliationsCount;
        hasTakenTurn = false;
    }

    // Математика шкоди як у Heroes of Might and Magic 3
    public int CalculateDamageTo(BattleUnit target)
    {
        // 1. Базова шкода стеку
        int rawDamage = 0;
        for (int i = 0; i < stackSize; i++)
        {
            rawDamage += Random.Range(data.minDamage, data.maxDamage + 1);
        }

        // 2. Модифікатор Атака vs Захист
        float modifier = 1.0f;
        int diff = data.attack - target.data.defense;

        if (diff > 0)
        {
            // Якщо Атака > Захисту: +5% за кожну одиницю різниці (макс +300%)
            modifier += diff * 0.05f;
            if (modifier > 4.0f) modifier = 4.0f;
        }
        else if (diff < 0)
        {
            // Якщо Атака < Захисту: -2.5% за кожну одиницю (макс -80%)
            modifier += diff * 0.025f;
            if (modifier < 0.2f) modifier = 0.2f;
        }

        return Mathf.Max(1, Mathf.RoundToInt(rawDamage * modifier));
    }

    // Отримання шкоди та перерахунок стеку
    public void TakeDamage(int damage)
    {
        // Рахуємо сумарне здоров'я всього стеку
        int totalStackHP = (stackSize - 1) * data.maxHealth + currentTopUnitHP;
        totalStackHP -= damage;

        if (totalStackHP <= 0)
        {
            stackSize = 0;
            currentTopUnitHP = 0;
            Die();
        }
        else
        {
            // Оновлюємо кількість живих юнітів
            stackSize = Mathf.CeilToInt((float)totalStackHP / data.maxHealth);
            currentTopUnitHP = totalStackHP % data.maxHealth;
            if (currentTopUnitHP == 0) currentTopUnitHP = data.maxHealth;
        }

        Debug.Log($"{data.unitName} отримав {damage} шкоди. Залишилося в стеку: {stackSize}");
    }

    private void Die()
    {
        Debug.Log($"Стек {data.unitName} повністю знищено!");
        gameObject.SetActive(false); // У грі тут запускається анімація смерті
    }
}