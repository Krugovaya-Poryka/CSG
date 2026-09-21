using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public GridBehavior grid;

    public GameObject pathMarkerPrefab;
    
    private GridStat previewTarget;
    
    public int teamId;
    
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
        UpdateGridPosition();
    }

    private void Update()
    {
        if (moving) 
            MoveAlongPath();
    }
    
    public GridStat lastTarget;

    private List<GameObject> pathMarkers = new List<GameObject>();
    
    public void PreviewPath(GridStat target)
    {
        Debug.Log("PREVIEW: " + target.x + ", " + target.y);
        UpdateGridPosition();

        if (previewTarget == target && currentPath != null)
        {
            ClearPathMarkers();
            currentPath = TrimPathByMovementPoints(currentPath);
            previewTarget = null;
            ConfirmMove();
            return;
        }

        ClearPathMarkers();

        List<GridStat> path = Pathfinder.FindPath(grid, gridX, gridY, target.x, target.y);
        
        Debug.Log("PATH COUNT: " + (path == null ? -1 : path.Count));
        
        if (path == null || path.Count == 0)
            return;

        previewTarget = target;
        currentPath = path;

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

            GameObject p = Instantiate(pathMarkerPrefab, tile.transform.position, Quaternion.identity);
            pathMarkers.Add(p);
            PathMaker pm = p.GetComponent<PathMaker>();
            
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
        int cost = to.movementCost;

        bool diagonal = from.x != to.x && from.y != to.y;
        if (diagonal)
            cost = Mathf.CeilToInt(cost * 1.4f);

        return cost;
    }

    private void MoveAlongPath()
    {
        if (currentPath == null || pathIndex >= currentPath.Count)
        {
            moving = false;
            UpdateGridPosition();
            return;
        }

        GridStat targetTile = currentPath[pathIndex];
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

            MapObject mapObject = targetTile.GetComponentInChildren<MapObject>();

            if (mapObject != null)
            {
                mapObject.Interact(this);
            }
            
            pathIndex++;
        }
    }

    private void UpdateGridPosition()
    {
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