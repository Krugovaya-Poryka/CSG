using UnityEngine;

public static class HexUtils
{
    // Обчислення відстані між двома гексами в осьових координатах (q, r)
    public static int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        // В гексагональній сітці відстань це: (|q1 - q2| + |r1 - r2| + |(q1 + r1) - (q2 + r2)|) / 2
        int dq = a.x - b.x;
        int dr = a.y - b.y;
        return (Mathf.Abs(dq) + Mathf.Abs(dr) + Mathf.Abs(dq + dr)) / 2;
    }
}