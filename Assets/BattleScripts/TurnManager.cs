using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<BattleUnit> allUnits = new List<BattleUnit>();
    private List<BattleUnit> turnQueue = new List<BattleUnit>();

    public BattleUnit activeUnit { get; private set; }
    public int currentRound = 0;

    public void StartBattle()
    {
        currentRound = 0;
        StartNewRound();
    }

    public void StartNewRound()
    {
        currentRound++;
        Debug.Log($"<color=yellow>=== СТАРТ РАУНДУ {currentRound} ===</color>");

        foreach (var unit in allUnits)
        {
            if (unit.stackSize > 0)
                unit.ResetForNewRound();
        }

        BuildTurnQueue();
        NextTurn();
    }

    private void BuildTurnQueue()
    {
        // Використовуємо C# LINQ для сортування (аналог std::sort в C++)
        // Сортуємо за Speed (за спаданням), а при рівності — за TeamId (0 ходитиме першим)
        turnQueue = allUnits
            .Where(u => u.stackSize > 0 && !u.hasTakenTurn)
            .OrderByDescending(u => u.data.speed)
            .ThenBy(u => u.teamId)
            .ToList();
    }

    public void NextTurn()
    {
        // Видаляємо вже мертвих зі списку
        turnQueue.RemoveAll(u => u.stackSize <= 0);

        if (turnQueue.Count == 0)
        {
            if (CheckBattleEnd()) return;

            StartNewRound();
            return;
        }

        activeUnit = turnQueue[0];
        turnQueue.RemoveAt(0);
        activeUnit.hasTakenTurn = true;

        Debug.Log($"Зараз ходить: {activeUnit.data.unitName} (Команда: {activeUnit.teamId}, Швидкість: {activeUnit.data.speed})");

        // Тут підсвічуємо гекси на відстані activeUnit.data.speed для гравця
    }

    private bool CheckBattleEnd()
    {
        bool team0Alive = allUnits.Any(u => u.teamId == 0 && u.stackSize > 0);
        bool team1Alive = allUnits.Any(u => u.teamId == 1 && u.stackSize > 0);

        if (!team0Alive || !team1Alive)
        {
            int winner = team0Alive ? 0 : 1;
            Debug.Log($"<color=green>Бій завершено! Перемогла команда {winner}</color>");
            return true;
        }
        return false;
    }
}