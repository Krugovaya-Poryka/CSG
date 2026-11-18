using UnityEngine;

public class ResourceObject : MapObject
{
    public ResourceType resourceType = ResourceType.Gold;
    [Min(1)] public int amount = 1000;
    public bool IsCollected { get; private set; }

    private void Awake()
    {
        UpdateAppearance();
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
        if (hero == null || IsCollected)
            return;
        TeamManager.EnsureExists();
        if (TeamManager.Instance == null)
            return;
        Team team = TeamManager.Instance.GetTeam(hero.teamId);

        if (team == null)
            return;

        if (!team.AddResource(resourceType, amount)) return;
        IsCollected = true;

        Debug.Log($"ResourceObject.Interact: +{amount} {ResourceCatalog.GetName(resourceType)}", this);
        StrategyHUD.NotifyPickup(hero.teamId, resourceType, amount);

        Destroy(gameObject);
    }
}
