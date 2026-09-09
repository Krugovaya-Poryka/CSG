using System.Collections.Generic;
using UnityEngine;

public class HeroController : MonoBehaviour
{
    public GridBehavior grid;

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

    public void MoveTo(GridStat target)
    {
        if (moving)
            return;

        UpdateGridPosition();

        List<GridStat> path = Pathfinder.FindPath(
            grid,
            gridX,
            gridY,
            target.x,
            target.y
        );

        if (path == null || path.Count == 0)
        {
            Debug.Log("Path not found");
            return;
        }

        currentPath = TrimPathByMovementPoints(path);

        if (currentPath.Count == 0)
        {
            Debug.Log("Not enough movement points");
            return;
        }

        pathIndex = 0;
        moving = true;
    }

    private List<GridStat> TrimPathByMovementPoints(
        List<GridStat> path)
    {
        List<GridStat> availablePath =
            new List<GridStat>();

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

    private int GetMovementCost(
        GridStat from,
        GridStat to)
    {
        int cost = to.movementCost;

        bool diagonal =
            from.x != to.x &&
            from.y != to.y;

        if (diagonal)
            cost = Mathf.CeilToInt(cost * 1.4f);

        return cost;
    }

    private void MoveAlongPath()
    {
        if (currentPath == null ||
            pathIndex >= currentPath.Count)
        {
            moving = false;
            UpdateGridPosition();
            return;
        }

        GridStat targetTile = currentPath[pathIndex];

        Vector3 target =
            targetTile.transform.position;

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            GridStat previous = grid.GetTile(gridX, gridY);

            int cost = GetMovementCost(previous, targetTile);

            movementPoints -= cost;

            transform.position = target;

            gridX = targetTile.x;
            gridY = targetTile.y;

            pathIndex++;
        }
    }

    private void UpdateGridPosition()
    {
        gridX = Mathf.RoundToInt(
            (transform.position.x -
             grid.leftBottomLocation.x) /
             grid.scale
        );

        gridY = Mathf.RoundToInt((transform.position.y - grid.leftBottomLocation.y) / grid.scale);
    }

    public void ResetMovement()
    {
        movementPoints = maxMovementPoints;
    }
}