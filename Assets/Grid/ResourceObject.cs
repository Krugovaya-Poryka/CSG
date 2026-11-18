using UnityEngine;

public class ResourceObject : MapObject
{
    public int amount = 1000;
    public bool IsCollected { get; private set; }

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

        IsCollected = true;
        team.gold += amount;

        Debug.Log("ResourceObject.Interact: Collected gold: " + amount, this);

        Destroy(gameObject);
    }
}
