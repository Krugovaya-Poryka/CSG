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
    public string playerHeroName; // Ім'я героя для описового тексту

    private bool isSurrendered = false;

    public void StartBattle()
    {
        registeredUnits = new List<BattleUnit>(allUnits);
        SortUnitsBySpeed();
        currentUnitIndex = 0;
        isSurrendered = false;
        StartTurn();
    }

    public void StartTurn()
    {
        if (CheckBattleEnd()) return;

        if (currentUnitIndex >= allUnits.Count)
        {
            allUnits.RemoveAll(u => u == null || u.stackSize <= 0);

            if (CheckBattleEnd()) return;

            currentUnitIndex = 0;
            SortUnitsBySpeed();

            foreach (var u in allUnits)
            {
                if (u != null) u.ResetRoundData();
            }
        }

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

        if (isSurrendered || team0Count == 0 || team1Count == 0)
        {
            activeUnit = null;

            if (resultUI != null)
            {
                BattleOutcome outcome;

                if (isSurrendered)
                {
                    outcome = BattleOutcome.Surrender;
                }
                else if (team1Count == 0)
                {
                    outcome = BattleOutcome.Victory;
                }
                else
                {
                    outcome = BattleOutcome.Defeat;
                }

                resultUI.ShowResults(outcome, registeredUnits, playerHeroName);
            }
            return true;
        }

        return false;
    }

    public void Surrender()
    {
        if (activeUnit == null) return;

        isSurrendered = true;

        foreach (var unit in allUnits)
        {
            if (unit != null && unit.teamId == 0)
            {
                unit.stackSize = 0;
                unit.UpdateStackText();
            }
        }

        CheckBattleEnd();
    }

    public void WaitCurrentUnit()
    {
        if (activeUnit == null || activeUnit.hasWaited) return;

        activeUnit.hasWaited = true;

        // Видаляємо з поточної позиції та додаємо у кінець списку
        allUnits.RemoveAt(currentUnitIndex);
        allUnits.Add(activeUnit);

        // Оскільки ми видалили елемент, наступний юніт автоматично став на currentUnitIndex.
        // Тому ми НЕ робимо currentUnitIndex++, а одразу запускаємо StartTurn().
        StartTurn();
    }

    public void SkipCurrentUnitTurn()
    {
        if (activeUnit == null) return;

        // Тут за бажанням можна додати +20% до захисту юніта (як у HoMM3):
        // activeUnit.ApplyDefenseBuff();

        EndTurn();
    }
}