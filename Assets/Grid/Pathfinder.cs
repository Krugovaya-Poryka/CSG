using System.Collections.Generic;
using UnityEngine;

public static class Pathfinder
{
    private class Node
    {
        public GridStat tile;
        public Node parent;

        public int gCost;
        public int hCost;

        public int fCost => gCost + hCost;
    }

    public static List<GridStat> FindPath(
        GridBehavior grid,
        int startX,
        int startY,
        int targetX,
        int targetY)
    {
        GridStat startTile = grid.GetTile(startX, startY);
        GridStat targetTile = grid.GetTile(targetX, targetY);

        if (startTile == null || targetTile == null)
            return null;

        if (!targetTile.walkable)
            return null;

        List<Node> openList = new List<Node>();
        HashSet<GridStat> closedSet = new HashSet<GridStat>();

        Dictionary<GridStat, Node> nodes =
            new Dictionary<GridStat, Node>();

        Node startNode = new Node
        {
            tile = startTile,
            gCost = 0,
            hCost = GetDistance(startTile, targetTile)
        };

        nodes[startTile] = startNode;
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node current = GetLowestCostNode(openList);

            if (current.tile == targetTile)
                return BuildPath(current);

            openList.Remove(current);
            closedSet.Add(current.tile);

            foreach (GridStat neighbour in grid.GetNeighbours(current.tile))
            {
                if (neighbour == null)
                    continue;

                if (!neighbour.walkable)
                    continue;

                if (closedSet.Contains(neighbour))
                    continue;

                int movementCost = neighbour.movementCost;

                // Діагональний рух трохи дорожчий
                bool diagonal =
                    neighbour.x != current.tile.x &&
                    neighbour.y != current.tile.y;

                int stepCost = movementCost * 10;

                if (diagonal)
                    stepCost = Mathf.RoundToInt(stepCost * 1.4f);

                int newGCost = current.gCost + stepCost;

                if (!nodes.TryGetValue(neighbour, out Node neighbourNode))
                {
                    neighbourNode = new Node
                    {
                        tile = neighbour,
                        gCost = int.MaxValue
                    };

                    nodes[neighbour] = neighbourNode;
                }

                if (newGCost < neighbourNode.gCost)
                {
                    neighbourNode.gCost = newGCost;
                    neighbourNode.hCost =
                        GetDistance(neighbour, targetTile);

                    neighbourNode.parent = current;

                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
            }
        }

        return null;
    }

    private static Node GetLowestCostNode(List<Node> list)
    {
        Node best = list[0];

        for (int i = 1; i < list.Count; i++)
        {
            Node node = list[i];

            if (node.fCost < best.fCost ||
                node.fCost == best.fCost &&
                node.hCost < best.hCost)
            {
                best = node;
            }
        }

        return best;
    }

    private static int GetDistance(GridStat a, GridStat b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        // Octile distance для 8 напрямків
        int diagonal = Mathf.Min(dx, dy);
        int straight = Mathf.Abs(dx - dy);

        return diagonal * 14 + straight * 10;
    }

    private static List<GridStat> BuildPath(Node endNode)
    {
        List<GridStat> path = new List<GridStat>();

        Node current = endNode;

        while (current.parent != null)
        {
            path.Add(current.tile);
            current = current.parent;
        }

        path.Reverse();

        return path;
    }
}