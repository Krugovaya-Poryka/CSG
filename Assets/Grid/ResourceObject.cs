using UnityEngine;

public class ResourceObject : MapObject
{
    public int amount = 1000;

    public override void Interact(HeroController hero)
    {
        Team team = TeamManager.Instance.GetTeam(hero.teamId);

        if (team == null)
            return;

        team.gold += amount;

        Debug.Log("Collected gold: " + amount);

        Destroy(gameObject);
    }
}