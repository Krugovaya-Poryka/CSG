using UnityEngine;

public class BattleHex : MonoBehaviour
{
    public int x;
    public int y;
    public bool walkable = true;

    public Vector2Int Coords => new Vector2Int(x, y);
}