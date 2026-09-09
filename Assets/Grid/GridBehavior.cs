using System.Collections.Generic;
using UnityEngine;

public class GridBehavior : MonoBehaviour
{
    [Header("Map")]
    public int seed = 113323;
    public int rows = 10;
    public int columns = 10;
    public int scale = 1;

    public GameObject gridPrefab;
    public Vector2 leftBottomLocation = Vector2.zero;

    public GameObject[,] gridArray;

    [Header("Generation")]
    public float noiseScale = 0.1f;

    private void Awake()
    {
        Template template = new Template();

        rows = template.height;
        columns = template.width;

        gridArray = new GameObject[columns, rows];

        if (gridPrefab != null)
        {
            GenerateGrid();
        }
        else
        {
            Debug.LogError("No grid prefab found!");
        }
    }

    private void GenerateGrid()
    {
        System.Random rng = new System.Random(seed);

        float seedOffsetX = rng.Next(-100000, 100000);
        float seedOffsetY = rng.Next(-100000, 100000);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                float value = Mathf.PerlinNoise(
                    (x + seedOffsetX) * noiseScale,
                    (y + seedOffsetY) * noiseScale
                );

                Vector2 worldPosition = new Vector2(
                    leftBottomLocation.x + scale * x,
                    leftBottomLocation.y + scale * y
                );

                GameObject obj = Instantiate(
                    gridPrefab,
                    worldPosition,
                    Quaternion.identity
                );

                obj.transform.SetParent(transform);

                GridStat stat = obj.GetComponent<GridStat>();

                if (stat == null)
                {
                    Debug.LogError(
                        "Grid prefab does not have GridStat component!"
                    );

                    Destroy(obj);
                    continue;
                }

                stat.x = x;
                stat.y = y;
                stat.grid = this;

                // Генерація типу землі
                if (value < 0.30f)
                {
                    stat.tileType = GridStat.TileType.Water;
                }
                else if (value < 0.65f)
                {
                    stat.tileType = GridStat.TileType.Grass;
                }
                else if (value < 0.82f)
                {
                    stat.tileType = GridStat.TileType.Forest;
                }
                else
                {
                    stat.tileType = GridStat.TileType.Mountain;
                }

                stat.ApplyTileType();
                stat.UpdateText();

                gridArray[x, y] = obj;
            }
        }
    }

    // Повертає клітинку за координатами
    public GridStat GetTile(int x, int y)
    {
        if (x < 0 || x >= columns ||
            y < 0 || y >= rows)
        {
            return null;
        }

        if (gridArray[x, y] == null)
        {
            return null;
        }

        return gridArray[x, y].GetComponent<GridStat>();
    }

    // Повертає сусідні клітинки в 8 напрямках
    public List<GridStat> GetNeighbours(GridStat tile)
    {
        List<GridStat> neighbours = new List<GridStat>();

        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetY = -1; offsetY <= 1; offsetY++)
            {
                if (offsetX == 0 && offsetY == 0)
                    continue;

                int neighbourX = tile.x + offsetX;
                int neighbourY = tile.y + offsetY;

                GridStat neighbour =
                    GetTile(neighbourX, neighbourY);

                if (neighbour == null)
                    continue;

                // Перевіряємо діагональний рух.
                // Не дозволяємо проходити крізь кут
                // між двома непрохідними клітинками.
                if (offsetX != 0 && offsetY != 0)
                {
                    GridStat horizontal =
                        GetTile(tile.x + offsetX, tile.y);

                    GridStat vertical =
                        GetTile(tile.x, tile.y + offsetY);

                    if (horizontal == null ||
                        vertical == null ||
                        !horizontal.walkable ||
                        !vertical.walkable)
                    {
                        continue;
                    }
                }

                neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }

    // Перетворює позицію у світі в координати клітинки
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.RoundToInt(
            (worldPosition.x - leftBottomLocation.x) / scale
        );

        int y = Mathf.RoundToInt(
            (worldPosition.y - leftBottomLocation.y) / scale
        );

        return new Vector2Int(x, y);
    }
}