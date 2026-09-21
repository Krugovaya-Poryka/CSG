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
    public bool canCastleSpawn = false;
    public GameObject castlePrefab;

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

    public enum Objective
    {
        Castle
    }

    public Sprite castleSprite;
    
    public TileType tileType;
    
    public Sprite[] grassSprites;
    
    public Sprite roadCenter;    
    public Sprite roadTop;
    public Sprite roadBottom;
    public Sprite roadLeft;
    public Sprite roadRight;
    public Sprite roadTopLeft;
    public Sprite roadTopRight;
    public Sprite roadBottomLeft;
    public Sprite roadBottomRight;
    public Sprite roadBottomLeftRight;
    public Sprite roadTopLeftRight;
    public Sprite roadBottomTopRight;
    public Sprite roadBottomTopLeft;
    public Sprite roadBottomTopLeftRight;
    
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
    public Sprite waterDiagonalBottomRight;
    public Sprite waterDiagonalBottomLeft;
    public Sprite waterDiagonalTopRight;
    public Sprite waterDiagonalTopLeft;




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
                canCastleSpawn = true;
                if (grassSprites.Length > 0)
                    spriteRenderer.sprite =
                        grassSprites[Random.Range(0, grassSprites.Length)];

                break;

            case TileType.Road:
                walkable = true;
                movementCost = 1;
                break;

            case TileType.Swamp:
                walkable = true;
                movementCost = 4;

                if (swampSprites.Length > 0)
                    spriteRenderer.sprite = swampSprites[Random.Range(0, swampSprites.Length)];

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

    public GameObject BuildCastle()
    {
        if (castlePrefab == null)
            return null;

        return Instantiate(castlePrefab, transform.position, Quaternion.identity, transform);
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
        if (tileType == TileType.Road)
        {
            UpdateRoadSprite();
        }
        
    }
    
    private void UpdateRoadSprite()
    {
        bool grassTop = !CheckTile(x, y + 1, TileType.Road);
        bool grassBottom = !CheckTile(x, y - 1, TileType.Road);
        bool grassLeft = !CheckTile(x - 1, y, TileType.Road);
        bool grassRight = !CheckTile(x + 1, y, TileType.Road);

        if (grassTop && grassBottom && grassRight && grassLeft)
        {
            spriteRenderer.sprite = roadBottomTopLeftRight;
            return;
        }
        
        if (grassTop && grassBottom && grassRight)
        {
            spriteRenderer.sprite = roadBottomTopRight;
            return;
        }

        if (grassBottom && grassLeft && grassTop)
        {
            spriteRenderer.sprite = roadBottomTopLeft;
            return;
        }
        
        if (grassTop && grassLeft && grassRight)
        {
            spriteRenderer.sprite = roadTopLeftRight;
            return;
        }

        if (grassBottom && grassLeft && grassRight)
        {
            spriteRenderer.sprite = roadBottomLeftRight;
            return;
        }

        if (grassTop && grassLeft)
        {
            spriteRenderer.sprite = roadTopLeft;
            return;
        }

        if (grassTop && grassRight)
        {
            spriteRenderer.sprite = roadTopRight;
            return;
        }

        if (grassBottom && grassLeft)
        {
            spriteRenderer.sprite = roadBottomLeft;
            return;
        }

        if (grassBottom && grassRight)
        {
            spriteRenderer.sprite = roadBottomRight;
            return;
        }

        if (grassTop)
        {
            spriteRenderer.sprite = roadTop;
            return;
        }

        if (grassBottom)
        {
            spriteRenderer.sprite = roadBottom;
            return;
        }

        if (grassLeft)
        {
            spriteRenderer.sprite = roadLeft;
            return;
        }

        if (grassRight)
        {
            spriteRenderer.sprite = roadRight;
            return;
        }
    
        spriteRenderer.sprite = roadCenter;
    }
    
    private void UpdateWaterSprite()
    {
        bool grassTop = CheckTile(x, y + 1, TileType.Water);
        bool grassBottom = CheckTile(x, y - 1, TileType.Water);
        bool grassLeft = CheckTile(x - 1, y, TileType.Water);
        bool grassRight = CheckTile(x + 1, y, TileType.Water);
        bool grassTopRight = CheckTile(x + 1, y + 1, TileType.Water);
        bool grassTopLeft = CheckTile(x - 1, y + 1, TileType.Water);
        bool grassBottomRight = CheckTile(x + 1, y - 1, TileType.Water);
        bool grassBottomLeft = CheckTile(x - 1, y - 1, TileType.Water);




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
        if (grassTopRight)
        {
            spriteRenderer.sprite = waterDiagonalTopRight;
            return;
        }

        if (grassTopLeft)
        {
            spriteRenderer.sprite = waterDiagonalTopLeft;
            return;
        }

        if (grassBottomRight)
        {
            spriteRenderer.sprite = waterDiagonalBottomRight;
            return;
        }

        if (grassBottomLeft)
        {
            spriteRenderer.sprite = waterDiagonalBottomLeft;
            return;
        }
        
        spriteRenderer.sprite = waterCenter;
    }
    
    
    private void UpdateForestSprite()
    {
        bool emptyTop = CheckTile(x, y + 1, TileType.Forest);
        bool emptyBottom = CheckTile(x, y - 1, TileType.Forest);
        bool emptyLeft = CheckTile(x - 1, y, TileType.Forest);
        bool emptyRight = CheckTile(x + 1, y, TileType.Forest);

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
    
    public bool CheckTile(int checkX, int checkY, TileType tile)
    {
        GridStat neighbour = grid.GetTile(checkX, checkY);
        
        if(neighbour == null)
            return true;
        
        return neighbour.tileType != tile;
    }
    
    
    public void UpdateText()
    {
        //coordinateText.text = x + ", " + y + "\n" + tileType.ToString();
    }
}