using UnityEngine;

public class Mine : TeamOwnedObject
{
    private void Start()
    {
        TeamManager.EnsureExists();
        if (TeamManager.Instance != null)
            TeamManager.Instance.RegisterMine(this);
    }

    private void OnDestroy()
    {
        if (TeamManager.Instance != null)
            TeamManager.Instance.UnregisterMine(this);
    }

    public override void Interact(HeroController hero)
    {
        if (hero == null)
            return;

        TeamManager.EnsureExists();
        bool captured = teamId != hero.teamId;
        teamId = hero.teamId;
        if (TeamManager.Instance != null)
            TeamManager.Instance.RegisterMine(this);

        Debug.Log(captured ? "Mine.Interact: Mine captured!" : "Mine.Interact: Visited own mine", this);
    }
}
