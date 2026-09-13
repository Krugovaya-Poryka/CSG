using System.Collections.Generic;
using UnityEngine;

public class GridBehavior : MonoBehaviour
{
    public int castleWidth = 3;
    public int castleHeight = 3;
    [System.Serializable]
    public class CastleInfo
    {
        public Vector2Int bottomLeft;
        public Vector2Int entrance;
    }

    public List<CastleInfo> castleInfos = new List<CastleInfo>();
    [Header("Map")]
    public int seed = 1;
    public int rows = 10;
    public int columns = 10;
    public int scale = 1;
    public int castles = 2;
    
    public GameObject gridPrefab;
    public Vector2 leftBottomLocation = Vector2.zero;

    public GameObject[,] gridArray;

    [Header("Generation")]
    public float noiseScale = 0.05f;

    private void Awake()
    {
        Template template = new Template();

        rows = template.height;
        columns = template.width;
        castles = template.castlesCount;

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
                float value = Mathf.PerlinNoise((x + seedOffsetX) * noiseScale, (y + seedOffsetY) * noiseScale);
                Vector2 worldPosition = new Vector2(leftBottomLocation.x + scale * x, leftBottomLocation.y + scale * y);
                GameObject obj = Instantiate(gridPrefab, worldPosition, Quaternion.identity);
                obj.transform.SetParent(transform);

                GridStat stat = obj.GetComponent<GridStat>();

                if (stat == null)
                {
                    Debug.LogError("Error!");

                    Destroy(obj);
                    continue;
                }

                stat.x = x;
                stat.y = y;
                stat.grid = this;

                if (value < 0.35f)
                {
                    stat.tileType = GridStat.TileType.Water;
                }
                else if (value < 0.65f)
                {
                    stat.tileType = GridStat.TileType.Grass;
                }
                else 
                {
                    stat.tileType = GridStat.TileType.Forest;
                }

                
                stat.ApplyTileType();
                stat.UpdateText();

                gridArray[x, y] = obj;
            }
        }

        CreateCastles();
        CreateRoads();
        UpdateAllTileSprites();   
    }

    private void CreateCastles()
    {
        System.Random rng = new System.Random(seed + 12345);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (castles <= 0)
                    return;

                GridStat tile = GetTile(x, y);

                if (tile == null)
                    continue;

                if (rng.NextDouble() >= 0.005f)
                    continue;

                if (!CanPlaceCastle(x, y))
                    continue;

                bool farEnough = true;

                foreach (CastleInfo castle in castleInfos)
                {
                    float distance = Vector2Int.Distance(
                        new Vector2Int(x, y),
                        castle.bottomLeft
                    );

                    if (distance < 40f)
                    {
                        farEnough = false;
                        break;
                    }
                }

                if (!farEnough)
                    continue;

                PlaceCastle(x, y);
                castles--;
            }
        }
    }

    private void PlaceCastle(int startX, int startY)
    {
        Vector2Int entrance = new Vector2Int(startX + castleWidth / 2, startY);

        for (int x = 0; x < castleWidth; x++)
        {
            for (int y = 0; y < castleHeight; y++)
            {
                GridStat tile = GetTile(startX + x, startY + y);
                tile.canCastleSpawn = false;

                if (tile.x == entrance.x && tile.y == entrance.y)
                {
                    tile.walkable = true;
                }
                else
                {
                    tile.walkable = false;
                }
            }
        }

        CastleInfo info = new CastleInfo();

        info.bottomLeft = new Vector2Int(startX, startY);
        info.entrance = entrance;
        castleInfos.Add(info);

        GridStat center = GetTile(startX + castleWidth / 2, startY + castleHeight / 2);

        center.BuildCastle();
    }
    
    private bool CanPlaceCastle(int startX, int startY)
    {
        for (int x = 0; x < castleWidth; x++)
        {
            for (int y = 0; y < castleHeight; y++)
            {
                GridStat tile = GetTile(
                    startX + x,
                    startY + y
                );

                if (tile == null)
                    return false;

                if (tile.tileType != GridStat.TileType.Grass)
                    return false;
            }
        }

        return true;
    }
    
    private void CreateRoads()
    {
        if (castleInfos.Count == 0)
            return;

        int sumX = 0;
        int sumY = 0;

        foreach (CastleInfo castle in castleInfos)
        {
            sumX += castle.entrance.x;
            sumY += castle.entrance.y;
        }

        Vector2Int center = new Vector2Int(sumX / castleInfos.Count, sumY / castleInfos.Count);

        List<CastleInfo> sorted =
            new List<CastleInfo>(castleInfos);

        sorted.Sort((a, b) => Vector2Int.Distance(a.entrance, center).CompareTo(Vector2Int.Distance(b.entrance, center)));

        List<Vector2Int> connected = new List<Vector2Int>();
        connected.Add(center);

        foreach (CastleInfo castle in sorted)
        {
            Vector2Int from = castle.entrance;
            Vector2Int nearest = connected[0];

            float nearestDistance = Vector2Int.Distance(from, nearest);
            
            foreach (Vector2Int target in connected)
            {
                float distance = Vector2Int.Distance(from, target);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = target;
                }
            }

            CreateRoadBetween(
                from,
                nearest
            );

            connected.Add(from);
        }
    }
    
    private void CreateRoadBetween(
        Vector2Int from,
        Vector2Int to)
    {
        List<GridStat> path = Pathfinder.FindPath(this, from.x, from.y, to.x, to.y, false);

        if (path == null)
            return;

        foreach (GridStat tile in path)
        {
            tile.tileType =
                GridStat.TileType.Road;

            tile.ApplyTileType();
            tile.UpdateText();
        }
    }

    private void UpdateAllTileSprites()
    {
        
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GridStat tile = GetTile(x, y);
                
                if (tile != null)
                    tile.UpdateSpriteByNeighbours();


            }
        }
    }
    
    public List<GridStat> GetRoadNeighbours(GridStat tile)
    {
        List<GridStat> neighbours = new List<GridStat>();

        GridStat up = GetTile(tile.x, tile.y + 1);
        GridStat down = GetTile(tile.x, tile.y - 1);
        GridStat left = GetTile(tile.x - 1, tile.y);
        GridStat right = GetTile(tile.x + 1, tile.y);

        if (up != null) neighbours.Add(up);
        if (down != null) neighbours.Add(down);
        if (left != null) neighbours.Add(left);
        if (right != null) neighbours.Add(right);

        return neighbours;
    }

    public GridStat GetTile(int x, int y)
    {
        if (x < 0 || x >= columns || y < 0 || y >= rows)
        {
            return null;
        }

        if (gridArray[x, y] == null)
        {
            return null;
        }

        return gridArray[x, y].GetComponent<GridStat>();
    }

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

                if (offsetX != 0 && offsetY != 0)
                {
                    GridStat horizontal = GetTile(tile.x + offsetX, tile.y);

                    GridStat vertical = GetTile(tile.x, tile.y + offsetY);

                    if (horizontal == null || vertical == null || !horizontal.walkable || !vertical.walkable)
                    {
                        continue;
                    }
                }

                neighbours.Add(neighbour);
            }
        }

        return neighbours;
    }

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