using UnityEngine;

public class ResourceObject : MapObject
{
    public ResourceType resourceType = ResourceType.Gold;
    [Min(1)] public int amount = 1000;
    [SerializeField, HideInInspector] private bool collected;
    [SerializeField, HideInInspector] private GridStat tile;
    public bool IsCollected => collected;
    public GridStat Tile => tile;

    private void Awake()
    {
        UpdateAppearance();
    }

    private void OnEnable()
    {
        tile = GetComponentInParent<GridStat>();
        if (tile == null || IsCollected) return;
        tile.resource = this;
        gridPosition = new Vector2Int(tile.x, tile.y);
    }

    private void OnDisable()
    {
        if (tile != null && tile.resource == this) tile.resource = null;
    }

    // Match navigation: diagonal collection cannot cut through two blocking cells.
    public bool CanCollectFrom(GridStat from)
    {
        if (IsCollected || tile == null || from == null || from.grid != tile.grid) return false;
        int dx = Mathf.Abs(from.x - tile.x), dy = Mathf.Abs(from.y - tile.y);
        if (Mathf.Max(dx, dy) != 1) return false;
        if (dx == 1 && dy == 1)
        {
            GridStat a = tile.grid.GetTile(from.x, tile.y);
            GridStat b = tile.grid.GetTile(tile.x, from.y);
            return a != null && b != null && a.IsTraversable && b.IsTraversable;
        }
        return true;
    }

    public bool CanCollect(HeroController hero)
    {
        return hero != null && hero.grid != null &&
            CanCollectFrom(hero.grid.GetTile(hero.gridX, hero.gridY));
    }

    public void Configure(ResourceType type, int quantity)
    {
        if (IsCollected) return;
        resourceType = type;
        amount = Mathf.Max(1, quantity);
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Sprite sprite = ResourceCatalog.GetSprite(resourceType);
        if (renderer != null && sprite != null)
        {
            renderer.sprite = sprite;
            renderer.sortingOrder = 3;
        }
        gameObject.name = ResourceCatalog.GetName(resourceType);
    }

    public override void Interact(HeroController hero)
    {
        if (!CanCollect(hero))
            return;
        TeamManager.EnsureExists();
        if (TeamManager.Instance == null)
            return;
        Team team = TeamManager.Instance.GetTeam(hero.teamId);

        if (team == null)
            return;

        if (!team.AddResource(resourceType, amount)) return;
        collected = true;
        if (tile != null && tile.resource == this) tile.resource = null;

        Debug.Log($"ResourceObject.Interact: +{amount} {ResourceCatalog.GetName(resourceType)}", this);
        StrategyHUD.NotifyPickup(hero.teamId, resourceType, amount);

        Destroy(gameObject);
    }
}
