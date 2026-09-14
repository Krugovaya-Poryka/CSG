using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<BattleUnit> allUnits = new List<BattleUnit>();
    public List<BattleUnit> registeredUnits = new List<BattleUnit>(); // Зберігає всіх юнітів для підрахунку втрат
    public BattleUnit activeUnit;
    private int currentUnitIndex = 0;

    public BattleResultUI resultUI; // Посилання на UI результатів

    public void StartBattle()
    {
        registeredUnits = new List<BattleUnit>(allUnits); // Фіксуємо всіх учасників
        SortUnitsBySpeed();
        currentUnitIndex = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        allUnits.RemoveAll(u => u == null || u.stackSize <= 0);

        if (CheckBattleEnd()) return;

        if (currentUnitIndex >= allUnits.Count)
        {
            currentUnitIndex = 0;
            SortUnitsBySpeed();

            foreach (var u in allUnits)
            {
                if (u != null) u.ResetRoundData();
            }
        }

        activeUnit = allUnits[currentUnitIndex];
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

        if (team0Count == 0 || team1Count == 0)
        {
            bool isPlayerVictory = (team1Count == 0);
            activeUnit = null; // Фікс: обнуляємо активного юніта, щоб він більше не рухався!

            if (resultUI != null)
            {
                resultUI.ShowResults(isPlayerVictory, registeredUnits);
            }
            return true;
        }

        return false;
    }
}