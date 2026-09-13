using UnityEngine;

public static class HexUtils
{
    // Точний розрахунок відстані в гексагональній сітці типу Odd-R
    public static int GetHexDistance(Vector2Int a, Vector2Int b)
    {
        int ax = a.x - (a.y - (a.y & 1)) / 2;
        int az = a.y;
        int ay = -ax - az;

        int bx = b.x - (b.y - (b.y & 1)) / 2;
        int bz = b.y;
        int by = -bx - bz;

        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by) + Mathf.Abs(az - bz)) / 2;
    }
}