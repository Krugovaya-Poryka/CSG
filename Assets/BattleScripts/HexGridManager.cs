using UnityEngine;
using System.Collections.Generic;

public class HexGridManager : MonoBehaviour
{
    [Header("Розміри поля бою HOMM3")]
    public int columns = 15;
    public int rows = 11;

    [Header("Налаштування гексів")]
    public GameObject hexPrefab;
    public float hexWidth = 0.866f;
    public float hexHeight = 1.0f;

    [Header("Перешкоди")]
    public Sprite[] obstacleSprites; // Перетягніть сюди спрайти каменів/дерев в Inspector
    int obstacleCount = 0;     // Кількість перешкод на полі

    public GameObject[,] gridArray;

    void Awake()
    {  
        obstacleCount = Random.Range(0, 10);
        GenerateHexGrid();
        GenerateObstacles();
    }

    void GenerateHexGrid()
    {
        gridArray = new GameObject[columns, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < columns; q++)
            {
                float xOffset = (r % 2 == 1) ? hexWidth * 0.5f : 0f;
                float xPos = q * hexWidth + xOffset;
                float yPos = r * (hexHeight * 0.75f);

                Vector2 spawnPosition = new Vector2(xPos, yPos);
                GameObject hex = Instantiate(hexPrefab, spawnPosition, Quaternion.identity, transform);
                hex.name = $"Hex_{q}_{r}";

                GridStat stat = hex.GetComponent<GridStat>();
                if (stat != null)
                {
                    stat.x = q;
                    stat.y = r;
                    stat.walkable = true;
                }

                gridArray[q, r] = hex;
            }
        }
    }

    void GenerateObstacles()
    {
        if (obstacleSprites == null || obstacleSprites.Length == 0) return;

        int spawned = 0;
        int attempts = 0;

        // Перешкоди спавняться в центральній частині поля (колонки 2..12)
        while (spawned < obstacleCount && attempts < 200)
        {
            attempts++;
            int q = Random.Range(2, columns - 2);
            int r = Random.Range(0, rows);

            GameObject hexObj = gridArray[q, r];
            GridStat stat = hexObj.GetComponent<GridStat>();

            if (stat != null && stat.walkable)
            {
                stat.walkable = false;

                // Встановлюємо спрайт перешкоди
                Transform inner = hexObj.transform.Find("InnerTile");
                SpriteRenderer sr = inner.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = obstacleSprites[Random.Range(0, obstacleSprites.Length)];
                    sr.color = Color.white; // Забезпечуємо повну видимість
                }

                spawned++;
            }
        }
    }

    public List<GameObject> GetHexNeighbors(int q, int r)
    {
        List<GameObject> neighbors = new List<GameObject>();
        bool isOdd = (r % 2 != 0);

        Vector2Int[] offsets = isOdd ? new Vector2Int[] {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1)
        } : new Vector2Int[] {
            new Vector2Int(1, 0), new Vector2Int(-1, 0),
            new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (var offset in offsets)
        {
            int nQ = q + offset.x;
            int nR = r + offset.y;

            if (nQ >= 0 && nQ < columns && nR >= 0 && nR < rows)
            {
                neighbors.Add(gridArray[nQ, nR]);
            }
        }

        return neighbors;
    }
}