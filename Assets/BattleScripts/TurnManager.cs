using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<BattleUnit> allUnits = new List<BattleUnit>();
    public List<BattleUnit> registeredUnits = new List<BattleUnit>();
    public BattleUnit activeUnit;
    private int currentUnitIndex = 0;

    public BattleResultUI resultUI;

    public void StartBattle()
    {
        registeredUnits = new List<BattleUnit>(allUnits);
        SortUnitsBySpeed();
        currentUnitIndex = 0;
        StartTurn();
    }

    public void StartTurn()
    {
        if (CheckBattleEnd()) return;

        // Перевірка завершення раунду
        if (currentUnitIndex >= allUnits.Count)
        {
            // Видаляємо мертвих юнітів ТІЛЬКИ наприкінці раунду
            allUnits.RemoveAll(u => u == null || u.stackSize <= 0);

            if (CheckBattleEnd()) return;

            currentUnitIndex = 0;
            SortUnitsBySpeed();

            foreach (var u in allUnits)
            {
                if (u != null) u.ResetRoundData();
            }
        }

        // Якщо юніт за цим індексом загинув раніше в цьому ж раунді — пропускаємо його
        if (allUnits[currentUnitIndex] == null || allUnits[currentUnitIndex].stackSize <= 0)
        {
            EndTurn();
            return;
        }

        activeUnit = allUnits[currentUnitIndex];

        if (BattleLogUI.Instance != null)
        {
            BattleLogUI.Instance.LogTurn(activeUnit);
        }
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
        int team0Count = allUnits.Count(u => u != null && u.teamId == 0 && u.stackSize > 0);
        int team1Count = allUnits.Count(u => u != null && u.teamId == 1 && u.stackSize > 0);

        if (team0Count == 0 || team1Count == 0)
        {
            bool isPlayerVictory = (team1Count == 0);
            activeUnit = null;

            if (resultUI != null)
            {
                resultUI.ShowResults(isPlayerVictory, registeredUnits);
            }
            return true;
        }

        return false;
    }
}