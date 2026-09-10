using TMPro;
using UnityEngine;

public class GridStat : MonoBehaviour
{
    public int visited = -1;
    public int x;
    public int y;
    public bool walkable = true;
    public TMP_Text coordinateText;
    public GridBehavior grid;
    public int movementCost;


    public enum TileType
    {
        Grass,
        Road,
        Mountain,
        Desert,
        Swamp,
        Snow,
        Forest,
        Water
    }
    public TileType tileType;
    
    public Sprite[] grassSprites;
    public Sprite[] roadSprites;
    
    public Sprite forestCenter;
    public Sprite forestTop;
    public Sprite forestBottom;
    public Sprite forestLeft;
    public Sprite forestRight;
    public Sprite forestTopLeft;
    public Sprite forestTopRight;
    public Sprite forestBottomLeft;
    public Sprite forestBottomRight;
    public Sprite forestTree;
    
    public Sprite[] swampSprites;
    
    public Sprite waterCenter;    
    public Sprite waterTop;
    public Sprite waterBottom;
    public Sprite waterLeft;
    public Sprite waterRight;
    public Sprite waterTopLeft;
    public Sprite waterTopRight;
    public Sprite waterBottomLeft;
    public Sprite waterBottomRight;

    private SpriteRenderer spriteRenderer;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ApplyTileType()
    {
        switch (tileType)
        {
            case TileType.Grass:
                walkable = true;
                movementCost = 2;

                if (grassSprites.Length > 0)
                    spriteRenderer.sprite =
                        grassSprites[Random.Range(0, grassSprites.Length)];

                break;

            case TileType.Road:
                walkable = true;
                movementCost = 1;

                if (roadSprites.Length > 0)
                    spriteRenderer.sprite =
                        roadSprites[Random.Range(0, roadSprites.Length)];

                break;

            case TileType.Swamp:
                walkable = true;
                movementCost = 4;

                if (swampSprites.Length > 0)
                    spriteRenderer.sprite =
                        swampSprites[Random.Range(0, swampSprites.Length)];

                break;

            case TileType.Water:
                walkable = false;
                movementCost = 999;
                break;

            case TileType.Forest:
                walkable = false;
                movementCost = 999;
                break;

            case TileType.Mountain:
                walkable = false;
                movementCost = 999;
                break;

            case TileType.Desert:
                walkable = true;
                movementCost = 3;
                break;

            case TileType.Snow:
                walkable = true;
                movementCost = 3;
                break;
        }
    }

    public void UpdateSpriteByNeighbours()
    {
        if (tileType == TileType.Water)
        {
            UpdateWaterSprite();
        }

        if (tileType == TileType.Forest)
        {
            UpdateForestSprite();
        }
    }
    
    private void UpdateWaterSprite()
    {
        bool grassTop = IsNotWater(x, y + 1);
        bool grassBottom = IsNotWater(x, y - 1);
        bool grassLeft = IsNotWater(x - 1, y);
        bool grassRight = IsNotWater(x + 1, y);

        if (grassTop && grassLeft)
        {
            spriteRenderer.sprite = waterTopLeft;
            return;
        }

        if (grassTop && grassRight)
        {
            spriteRenderer.sprite = waterTopRight;
            return;
        }

        if (grassBottom && grassLeft)
        {
            spriteRenderer.sprite = waterBottomLeft;
            return;
        }

        if (grassBottom && grassRight)
        {
            spriteRenderer.sprite = waterBottomRight;
            return;
        }

        if (grassTop)
        {
            spriteRenderer.sprite = waterTop;
            return;
        }

        if (grassBottom)
        {
            spriteRenderer.sprite = waterBottom;
            return;
        }

        if (grassLeft)
        {
            spriteRenderer.sprite = waterLeft;
            return;
        }

        if (grassRight)
        {
            spriteRenderer.sprite = waterRight;
            return;
        }
    
        spriteRenderer.sprite = waterCenter;
    }
    
    private bool IsNotWater(int checkX, int checkY)
    {
        GridStat neighbour = grid.GetTile(checkX, checkY);

        if (neighbour == null)
            return false;

        return neighbour.tileType == TileType.Grass;
    }
    
    private void UpdateForestSprite()
    {
        bool emptyTop = IsNotForest(x, y + 1);
        bool emptyBottom = IsNotForest(x, y - 1);
        bool emptyLeft = IsNotForest(x - 1, y);
        bool emptyRight = IsNotForest(x + 1, y);

        if (emptyRight && emptyBottom && emptyLeft && emptyTop)
        {
            spriteRenderer.sprite = forestTree;
            return;
        }
        
        if (emptyTop && emptyRight)
        {
            spriteRenderer.sprite = forestTopRight;
            return;
        }

        if (emptyBottom && emptyRight)
        {
            spriteRenderer.sprite = forestBottomRight;
            return;
        }

        if (emptyTop && emptyLeft)
        {
            spriteRenderer.sprite = forestTopLeft;
            return;
        }

        if (emptyBottom && emptyLeft)
        {
            spriteRenderer.sprite =  forestBottomLeft;
            return;
        }

        if (emptyTop)
        {
            spriteRenderer.sprite = forestTop;
            return;
        }

        if (emptyBottom)
        {
            spriteRenderer.sprite = forestBottom;
            return;
        }

        if (emptyLeft)
        {
            spriteRenderer.sprite = forestLeft;
            return;
        }

        if (emptyRight)
        {
            spriteRenderer.sprite = forestRight;
            return;
        }



        spriteRenderer.sprite = forestCenter;
    }
    
    private bool IsNotForest(int checkX, int checkY)
    {
        GridStat neighbour = grid.GetTile(checkX, checkY);
        
        if(neighbour == null)
            return false;
        
        return neighbour.tileType == TileType.Grass;
    }
    
    public void UpdateText()
    {
        //coordinateText.text = x + ", " + y + "\n" + tileType.ToString();
    }
}