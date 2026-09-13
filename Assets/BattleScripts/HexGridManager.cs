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

    [Header("Налаштування сітки ліній")]
    public Color gridLineColor = new Color(0f, 0f, 0f, 1f); // Колір сітки
    public float gridLineWidth = 0.05f;                       // Товщина лінії
    public Material lineMaterial;                             // Матеріал (наприклад, Sprites/Default)

    void Awake()
    {  
        obstacleCount = Random.Range(0, 10);
        GenerateHexGrid();
        GenerateObstacles();
        DrawGridLines();
    }

    void DrawGridLines()
    {
        GameObject linesParent = new GameObject("GridLines");
        linesParent.transform.SetParent(transform);

        // Хеш-сет для збереження вже намальованих граней (усуває подвійну товщину)
        HashSet<(Vector2Int, Vector2Int)> drawnEdges = new HashSet<(Vector2Int, Vector2Int)>();

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < columns; q++)
            {
                Vector2 center = gridArray[q, r].transform.position;
                Vector2[] corners = GetHexCorners(center);

                for (int i = 0; i < 6; i++)
                {
                    Vector2 p1 = corners[i];
                    Vector2 p2 = corners[(i + 1) % 6];

                    // Округлюємо координати для виключення помилок float
                    Vector2Int ip1 = new Vector2Int(Mathf.RoundToInt(p1.x * 1000f), Mathf.RoundToInt(p1.y * 1000f));
                    Vector2Int ip2 = new Vector2Int(Mathf.RoundToInt(p2.x * 1000f), Mathf.RoundToInt(p2.y * 1000f));

                    // Канонічний ключ відрізка (p1 завжди "менше" p2)
                    var edgeKey = (ip1.x < ip2.x || (ip1.x == ip2.x && ip1.y < ip2.y))
                        ? (ip1, ip2)
                        : (ip2, ip1);

                    if (!drawnEdges.Contains(edgeKey))
                    {
                        drawnEdges.Add(edgeKey);
                        CreateLineSegment(p1, p2, linesParent.transform);
                    }
                }
            }
        }
    }

    Vector2[] GetHexCorners(Vector2 center)
    {
        Vector2[] corners = new Vector2[6];
        float radius = hexHeight / 2.0f;

        for (int i = 0; i < 6; i++)
        {
            // 30, 90, 150... градусів для Pointy-Topped гексів HOMM3
            float angleDeg = 60f * i + 30f;
            float angleRad = angleDeg * Mathf.Deg2Rad;

            corners[i] = new Vector2(
                center.x + radius * Mathf.Cos(angleRad),
                center.y + radius * Mathf.Sin(angleRad)
            );
        }
        return corners;
    }

    void CreateLineSegment(Vector2 p1, Vector2 p2, Transform parent)
    {
        GameObject lineObj = new GameObject("GridEdge");
        lineObj.transform.SetParent(parent);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial != null ? lineMaterial : new Material(Shader.Find("Sprites/Default"));
        lr.startColor = gridLineColor;
        lr.endColor = gridLineColor;
        lr.startWidth = gridLineWidth;
        lr.endWidth = gridLineWidth;
        lr.positionCount = 2;
        lr.useWorldSpace = true;

        // Z = -0.01f виносить лінію трохи вперед перед спрайтами гексів
        lr.SetPosition(0, new Vector3(p1.x, p1.y, -0.01f));
        lr.SetPosition(1, new Vector3(p2.x, p2.y, -0.01f));
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