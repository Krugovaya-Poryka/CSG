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
    public Sprite[] swampSprites;
    public Sprite[] waterSprites;

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

                if (waterSprites.Length > 0)
                    spriteRenderer.sprite = waterSprites[Random.Range(0, waterSprites.Length)];

                break;

            case TileType.Forest:
                walkable = true;
                movementCost = 3;
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

    public void UpdateText()
    {
        coordinateText.text = x + ", " + y + "\n" + tileType.ToString();
    }
}