using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<BattleUnit> allUnits = new List<BattleUnit>();
    public BattleUnit activeUnit;
    private int currentUnitIndex = 0;

    public void StartBattle()
    {
        // 1. Сортуємо юнітів за швидкістю (найшвидші ходять першими)
        SortUnitsBySpeed();
        currentUnitIndex = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        // 1. Очищаємо список від знищених юнітів
        allUnits.RemoveAll(u => u == null || u.stackSize <= 0);

        // 2. Перевіряємо, чи не закінчився бій
        if (CheckBattleEnd()) return;

        // 3. Якщо пройшли всі юніти — починається новий раунд
        if (currentUnitIndex >= allUnits.Count)
        {
            currentUnitIndex = 0;

            // Пересортовуємо за швидкістю на новий раунд
            SortUnitsBySpeed();

            // Відновлюємо контратаки для кожного юніта відповідно до його UnitData
            foreach (var u in allUnits)
            {
                if (u != null) u.ResetRoundData();
            }

            Debug.Log("================ ПОЧАТОК НОВОГО РАУНДУ ================");
        }

        // 4. Встановлюємо активного юніта
        activeUnit = allUnits[currentUnitIndex];
        Debug.Log($"---> ХІД: {activeUnit.data.unitName} (Команда {activeUnit.teamId}, Швидкість {activeUnit.data.speed})");
    }

    public void EndTurn()
    {
        currentUnitIndex++;
        StartTurn();
    }

    private void SortUnitsBySpeed()
    {
        allUnits = allUnits.OrderByDescending(u => u.data.speed).ToList();
    }

    private bool CheckBattleEnd()
    {
        int team0Count = allUnits.Count(u => u.teamId == 0 && u.stackSize > 0);
        int team1Count = allUnits.Count(u => u.teamId == 1 && u.stackSize > 0);

        if (team0Count == 0)
        {
            Debug.Log("=== БІЙ ЗАВЕРШЕНО: ПЕРЕМОГА ВОРОГА! ===");
            return true;
        }
        if (team1Count == 0)
        {
            Debug.Log("=== БІЙ ЗАВЕРШЕНО: ПЕРЕМОГА ГРАВЦЯ! ===");
            return true;
        }

        return false;
    }
}