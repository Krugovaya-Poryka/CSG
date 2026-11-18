using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public GridBehavior grid;

    public GameObject pathMarkerPrefab;
    
    private GridStat previewTarget;
    private ResourceObject pendingResource;
    
    public int teamId;
    
    [Header("Hero composition")]
    public HeroStats stats = new HeroStats();
    public HeroInventory inventory = new HeroInventory();
    // Assign in the Inspector, or use an existing component on this hero/its children.
    public HeroArmy army;

    public int Attack => (stats != null ? stats.attack : 0) + (inventory != null ? inventory.AttackBonus : 0);
    public int Defense => (stats != null ? stats.defense : 0) + (inventory != null ? inventory.DefenseBonus : 0);

    private void Awake()
    {
        if (stats == null)
            stats = new HeroStats();
        if (inventory == null)
            inventory = new HeroInventory();
        if (army == null)
            army = GetComponentInChildren<HeroArmy>(true);
    }

    [Header("Movement")]
    public int maxMovementPoints = 30;
    public int movementPoints = 30;

    public float moveSpeed = 4f;

    [Header("Grid Position")]
    public int gridX;
    public int gridY;

    public bool selected;

    private List<GridStat> currentPath;
    private int pathIndex;

    private bool moving;
    
    
    private void Start()
    {
        TeamManager.EnsureExists();
        if (TeamManager.Instance != null)
            TeamManager.Instance.RegisterHero(this);
        HeroDetailsPanel.EnsureExists();
        UpdateGridPosition();
    }

    private void OnDestroy()
    {
        if (TeamManager.Instance != null)
            TeamManager.Instance.UnregisterHero(this);
    }

    private void Update()
    {
        if (moving) 
            MoveAlongPath();
    }
    
    private List<GameObject> pathMarkers = new List<GameObject>();
    
    public void PreviewPath(GridStat target)
    {
        if (target == null || grid == null || moving)
            return;
        Debug.Log("PREVIEW: " + target.x + ", " + target.y);
        UpdateGridPosition();

        ResourceObject resource = target.resource;
        if (resource != null && resource.CanCollect(this))
        {
            ClearPathMarkers();
            previewTarget = null;
            currentPath = null;
            pendingResource = null;
            MapObject interaction = resource;
            interaction.Interact(this);
            return;
        }

        if (previewTarget == target && currentPath != null)
        {
            ClearPathMarkers();
            currentPath = TrimPathByMovementPoints(currentPath);
            previewTarget = null;
            ConfirmMove();
            return;
        }

        ClearPathMarkers();

        previewTarget = null;
        currentPath = null;
        pendingResource = null;
        List<GridStat> path = resource != null && !resource.IsCollected
            ? FindResourceApproachPath(resource)
            : Pathfinder.FindPath(grid, gridX, gridY, target.x, target.y);
        
        Debug.Log("PATH COUNT: " + (path == null ? -1 : path.Count));
        
        if (path == null || path.Count == 0)
            return;

        previewTarget = target;
        currentPath = path;
        pendingResource = resource;

        int points = movementPoints;

        GridStat previous = grid.GetTile(gridX, gridY);

        for (int i = 0; i < path.Count; i++)
        {
            GridStat tile = path[i];

            if (tile.x == gridX && tile.y == gridY)
            {
                previous = tile;
                continue;
            }

            int cost = GetMovementCost(previous, tile);

            if (pathMarkerPrefab == null)
            {
                previous = tile;
                continue;
            }

            GameObject p = Instantiate(pathMarkerPrefab, tile.transform.position, Quaternion.identity);
            pathMarkers.Add(p);
            PathMaker pm = p.GetComponent<PathMaker>();
            if (pm == null)
            {
                previous = tile;
                continue;
            }
            
            bool isEnd = i == path.Count - 1;

            if (points >= cost)
            {
                if (isEnd)
                    pm.SetGreenEnd();
                else
                    pm.SetGreen();
            }
            else
            {
                if (isEnd)
                    pm.SetRedEnd();
                else
                    pm.SetRed();
            }
            
            points -= cost;
            if (i < path.Count - 1)
            {
                GridStat next = path[i + 1];

                int dx = next.x - tile.x;
                int dy = next.y - tile.y;

                float angle = 0;

                if (dx == 0 && dy > 0) angle = 0;
                else if (dx > 0 && dy > 0) angle = -45;
                else if (dx > 0 && dy == 0) angle = -90;
                else if (dx > 0 && dy < 0) angle = -135;
                else if (dx == 0 && dy < 0) angle = 180;
                else if (dx < 0 && dy < 0) angle = 135;
                else if (dx < 0 && dy == 0) angle = 90;
                else if (dx < 0 && dy > 0) angle = 45;

                p.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
 

            previous = tile;
        }
    }
    private List<GridStat> FindResourceApproachPath(ResourceObject resource)
    {
        if (resource.Tile == null) return null;
        List<GridStat> best = null;
        int bestCost = int.MaxValue;
        foreach (GridStat approach in grid.GetNeighbours(resource.Tile))
        {
            if (!approach.IsTraversable || !resource.CanCollectFrom(approach)) continue;
            List<GridStat> path = Pathfinder.FindPath(grid, gridX, gridY, approach.x, approach.y);
            if (path == null || path.Count == 0) continue;
            int cost = 0;
            GridStat previous = grid.GetTile(gridX, gridY);
            foreach (GridStat step in path) { cost += GetMovementCost(previous, step); previous = step; }
            if (cost < bestCost) { best = path; bestCost = cost; }
        }
        return best;
    }

    private void CollectPendingResource()
    {
        MapObject interaction = pendingResource;
        pendingResource = null;
        if (interaction != null) interaction.Interact(this);
    }

    private void ClearPathMarkers()
    {
        foreach (GameObject marker in pathMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }

        pathMarkers.Clear();
    }
    
    
    public void ConfirmMove()
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        pathIndex = 0;
        moving = true;
    }

    private List<GridStat> TrimPathByMovementPoints(List<GridStat> path)
    {
        List<GridStat> availablePath = new List<GridStat>();

        int remaining = movementPoints;
        GridStat previous = grid.GetTile(gridX, gridY);

        foreach (GridStat tile in path)
        {
            if (tile == null || !tile.IsTraversable) break;
            int cost = GetMovementCost(previous, tile);

            if (remaining < cost)
                break;

            remaining -= cost;
            availablePath.Add(tile);
            previous = tile;
        }

        return availablePath;
    }

    private int GetMovementCost(GridStat from, GridStat to)
    {
        if (to == null)
            return 0;
        int cost = to.movementCost;

        bool diagonal = from != null && from.x != to.x && from.y != to.y;
        if (diagonal)
            cost = Mathf.CeilToInt(cost * 1.4f);

        return cost;
    }

    private void MoveAlongPath()
    {
        if (grid == null || currentPath == null || pathIndex >= currentPath.Count)
        {
            moving = false;
            UpdateGridPosition();
            return;
        }

        GridStat targetTile = currentPath[pathIndex];
        if (targetTile == null || !targetTile.IsTraversable)
        {
            pendingResource = null;
            moving = false;
            return;
        }
        Vector3 target = targetTile.transform.position;
        transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            GridStat previous = grid.GetTile(gridX, gridY);

            int cost = GetMovementCost(previous, targetTile);

            movementPoints -= cost;

            transform.position = target;

            gridX = targetTile.x;
            gridY = targetTile.y;

            // The entrance link takes priority even when the castle sprite is on another tile.
            MapObject mapObject = targetTile.castle;
            if (mapObject == null)
                mapObject = targetTile.GetComponentInChildren<MapObject>();
            if (mapObject != null)
                mapObject.Interact(this);
            
            pathIndex++;
            if (pathIndex >= currentPath.Count) CollectPendingResource();
        }
    }

    private void UpdateGridPosition()
    {
        if (grid == null)
            return;
        Vector2Int pos = grid.WorldToGrid(transform.position);

        gridX = pos.x;
        gridY = pos.y;

        Debug.Log("HERO GRID: " + gridX + ", " + gridY);
    }

    public void ResetMovement()
    {
        movementPoints = maxMovementPoints;
    }
}
