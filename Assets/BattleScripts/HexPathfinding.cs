using System.Collections.Generic;
using UnityEngine;

public static class HexPathfinding
{
    public static List<Vector2Int> GetNeighbors(Vector2Int hex)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        bool isEvenRow = hex.y % 2 == 0;

        int[,] offsets = isEvenRow ? new int[,] {
            { -1,  0 }, { +1,  0 },
            { -1, +1 }, {  0, +1 },
            { -1, -1 }, {  0, -1 }
        } : new int[,] {
            { -1,  0 }, { +1,  0 },
            {  0, +1 }, { +1, +1 },
            {  0, -1 }, { +1, -1 }
        };

        for (int i = 0; i < 6; i++)
        {
            neighbors.Add(new Vector2Int(hex.x + offsets[i, 0], hex.y + offsets[i, 1]));
        }

        return neighbors;
    }

    public static Dictionary<Vector2Int, Vector2Int> FindReachableArea(
        Vector2Int start,
        int speed,
        bool isFlyer,
        HashSet<Vector2Int> blockedHexes)
    {
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        cameFrom[start] = start;

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        var costSoFar = new Dictionary<Vector2Int, int>();
        costSoFar[start] = 0;

        while (frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            if (costSoFar[current] >= speed) continue;

            foreach (Vector2Int next in GetNeighbors(current))
            {
                // Наземні юніти не можуть проходити крізь перешкоди/юнітів.
                // Літаючі юніти ігнорують перешкоди ПІД ЧАС переміщення.
                if (!isFlyer && blockedHexes.Contains(next)) continue;

                int newCost = costSoFar[current] + 1;
                if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                {
                    costSoFar[next] = newCost;
                    frontier.Enqueue(next);
                    cameFrom[next] = current;
                }
            }
        }

        return cameFrom;
    }

    public static List<Vector2Int> ReconstructPath(
        Vector2Int start,
        Vector2Int target,
        Dictionary<Vector2Int, Vector2Int> cameFrom,
        bool isFlyer)
    {
        List<Vector2Int> path = new List<Vector2Int>();

        if (isFlyer)
        {
            path.Add(target);
            return path;
        }

        if (!cameFrom.ContainsKey(target)) return path;

        Vector2Int current = target;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }
}