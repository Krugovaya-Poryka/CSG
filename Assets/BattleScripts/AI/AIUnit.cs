using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIUnit : MonoBehaviour
{
    private BattleUnit myUnit;
    private HexGridManager gridManager;

    private void Awake()
    {
        myUnit = GetComponent<BattleUnit>();
        // Замінено на FindAnyObjectByType
        gridManager = FindAnyObjectByType<HexGridManager>();
    }

    public bool IsAlive()
    {
        return myUnit != null && myUnit.stackSize > 0;
    }

    /// <summary>
    /// Головний метод прийняття рішення ШІ для покрокового бою HoMM3
    /// </summary>
    public void MakeAutonomousDecision(List<BattleUnit> allUnits, Action onTurnComplete)
    {
        if (!IsAlive())
        {
            onTurnComplete?.Invoke();
            return;
        }

        if (gridManager == null) gridManager = FindFirstObjectByType<HexGridManager>();

        // 1. Пошук ворожих юнітів
        List<BattleUnit> enemies = new List<BattleUnit>();
        foreach (var u in allUnits)
        {
            if (u != null && u.stackSize > 0 && u.teamId != myUnit.teamId)
            {
                enemies.Add(u);
            }
        }

        if (enemies.Count == 0)
        {
            onTurnComplete?.Invoke();
            return;
        }

        // 2. Вибір найближчого ворога (за відстані на гексовій сітці)
        BattleUnit target = GetClosestEnemy(enemies);
        if (target == null)
        {
            onTurnComplete?.Invoke();
            return;
        }

        int distance = HexUtils.GetHexDistance(myUnit.hexCoords, target.hexCoords);
        bool isEnemyAdjacent = CheckIfEnemyIsAdjacent(allUnits);

        // --- ВАРІАНТ A: Дальній бій ---
        if (myUnit.data.isRanged && myUnit.currentShots > 0 && !isEnemyAdjacent && distance > 1)
        {
            myUnit.RangedAttack(target, () => onTurnComplete?.Invoke());
            return;
        }

        // --- ВАРІАНТ Б: Ближній бій упритул ---
        if (distance == 1)
        {
            myUnit.MeleeAttack(target, () => onTurnComplete?.Invoke());
            return;
        }

        // --- ВАРІАНТ В: Переміщення по гексах + Атака або Крок убік ворога ---
        ExecuteMoveAndAttack(target, allUnits, onTurnComplete);
    }

    private void ExecuteMoveAndAttack(BattleUnit target, List<BattleUnit> allUnits, Action onTurnComplete)
    {
        HashSet<Vector2Int> blockedHexes = GetBlockedHexes(allUnits);

        // Пошук досяжних гексів з урахуванням швидкості та типів юніта (літаючий/наземний)
        var pathMap = HexPathfinding.FindReachableArea(
            myUnit.hexCoords,
            myUnit.data.speed,
            myUnit.data.isFlyer,
            blockedHexes
        );

        Vector2Int? bestAttackCoords = GetBestAttackHex(target, pathMap);

        if (bestAttackCoords.HasValue)
        {
            // Рух до сусіднього з ворогом гекса + ближній удар
            Vector2Int targetCoords = bestAttackCoords.Value;
            List<Vector2Int> pathCoords = HexPathfinding.ReconstructPath(
                myUnit.hexCoords, targetCoords, pathMap, myUnit.data.isFlyer
            );

            List<GameObject> pathHexes = ConvertCoordsToGameObjects(pathCoords);

            StartCoroutine(myUnit.MoveAlongPath(pathHexes, targetCoords, () =>
            {
                myUnit.MeleeAttack(target, () => onTurnComplete?.Invoke());
            }));
        }
        else
        {
            // Якщо ворог за межами ходу — робимо крок якомога ближче до нього
            Vector2Int? bestStep = GetBestStepTowards(target, pathMap);
            if (bestStep.HasValue && bestStep.Value != myUnit.hexCoords)
            {
                Vector2Int targetCoords = bestStep.Value;
                List<Vector2Int> pathCoords = HexPathfinding.ReconstructPath(
                    myUnit.hexCoords, targetCoords, pathMap, myUnit.data.isFlyer
                );

                List<GameObject> pathHexes = ConvertCoordsToGameObjects(pathCoords);

                StartCoroutine(myUnit.MoveAlongPath(pathHexes, targetCoords, () => onTurnComplete?.Invoke()));
            }
            else
            {
                // Немає шляху або юніт заблокований — пропускаємо хід
                onTurnComplete?.Invoke();
            }
        }
    }

    private BattleUnit GetClosestEnemy(List<BattleUnit> enemies)
    {
        BattleUnit closest = null;
        int minDist = int.MaxValue;

        foreach (var enemy in enemies)
        {
            int dist = HexUtils.GetHexDistance(myUnit.hexCoords, enemy.hexCoords);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    private HashSet<Vector2Int> GetBlockedHexes(List<BattleUnit> allUnits)
    {
        HashSet<Vector2Int> blocked = new HashSet<Vector2Int>();

        // Замінено на FindObjectsByType без FindObjectsSortMode
        BattleHex[] hexes = FindObjectsByType<BattleHex>(FindObjectsInactive.Exclude);

        foreach (var h in hexes)
        {
            if (!h.walkable) blocked.Add(new Vector2Int(h.x, h.y));
        }

        foreach (var u in allUnits)
        {
            if (u != null && u.stackSize > 0)
            {
                blocked.Add(u.hexCoords);
            }
        }
        return blocked;
    }

    private Vector2Int? GetBestAttackHex(BattleUnit target, Dictionary<Vector2Int, Vector2Int> pathMap)
    {
        List<Vector2Int> neighbors = HexPathfinding.GetNeighbors(target.hexCoords);
        Vector2Int? best = null;
        int minDist = int.MaxValue;

        foreach (var coords in neighbors)
        {
            if (!pathMap.ContainsKey(coords)) continue;

            int dist = HexUtils.GetHexDistance(myUnit.hexCoords, coords);
            if (dist < minDist)
            {
                minDist = dist;
                best = coords;
            }
        }
        return best;
    }

    private Vector2Int? GetBestStepTowards(BattleUnit target, Dictionary<Vector2Int, Vector2Int> pathMap)
    {
        Vector2Int? best = null;
        int minDist = int.MaxValue;

        foreach (var pair in pathMap)
        {
            int dist = HexUtils.GetHexDistance(pair.Key, target.hexCoords);
            if (dist < minDist)
            {
                minDist = dist;
                best = pair.Key;
            }
        }
        return best;
    }

    private List<GameObject> ConvertCoordsToGameObjects(List<Vector2Int> coordsList)
    {
        List<GameObject> list = new List<GameObject>();
        if (gridManager == null || gridManager.gridArray == null) return list;

        foreach (var c in coordsList)
        {
            if (c.x >= 0 && c.x < gridManager.columns && c.y >= 0 && c.y < gridManager.rows)
            {
                list.Add(gridManager.gridArray[c.x, c.y]);
            }
        }
        return list;
    }

    private bool CheckIfEnemyIsAdjacent(List<BattleUnit> allUnits)
    {
        foreach (var u in allUnits)
        {
            if (u != null && u.stackSize > 0 && u.teamId != myUnit.teamId)
            {
                if (HexUtils.GetHexDistance(myUnit.hexCoords, u.hexCoords) == 1) return true;
            }
        }
        return false;
    }
}