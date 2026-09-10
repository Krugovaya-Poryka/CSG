using UnityEngine;
using System.Collections.Generic;

public class HexGridManager : MonoBehaviour
{
    [Header("Розміри поля бою HOMM3")]
    public int columns = 15; // 15 колонок
    public int rows = 11;    // 11 рядків

    [Header("Налаштування гексів")]
    public GameObject hexPrefab; // Префаб одного гексагона (із колайдером і спрайтом)
    public float hexWidth = 0.866f;  // Ширина гекса
    public float hexHeight = 1.0f; // Висота гекса (стандартне співвідношення для гексів)

    public GameObject[,] gridArray;

    void Awake()
    {
        GenerateHexGrid();
    }

    void GenerateHexGrid()
    {
        gridArray = new GameObject[columns, rows];

        for (int r = 0; r < rows; r++)
        {
            for (int q = 0; q < columns; q++)
            {
                // Позиціонування гексагонів:
                // У непарних рядках робимо зсув по X на половину ширини гекса
                float xOffset = (r % 2 == 1) ? hexWidth * 0.5f : 0f;
                float xPos = q * hexWidth + xOffset;

                // По вертикалі гекси заходять один під одного на 25% (0.75 від висоти)
                float yPos = r * (hexHeight * 0.75f);

                Vector2 spawnPosition = new Vector2(xPos, yPos);

                // Створюємо гекс
                GameObject hex = Instantiate(hexPrefab, spawnPosition, Quaternion.identity, transform);
                hex.name = $"Hex_{q}_{r}";

                // Прив'язуємо координати (як у скрипті твоєї сітки)
                GridStat stat = hex.GetComponent<GridStat>();
                if (stat != null)
                {
                    stat.x = q;
                    stat.y = r;
                    //stat.UpdateText();
                }

                gridArray[q, r] = hex;
            }
        }
    }

    public List<GameObject> GetHexNeighbors(int q, int r)
    {
        List<GameObject> neighbors = new List<GameObject>();

        // Зсуви залежать від парності рядка r
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