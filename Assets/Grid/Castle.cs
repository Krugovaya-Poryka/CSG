using UnityEngine;

public class Castle : TeamOwnedObject
{
    // The castle sprite stays at the footprint centre; interaction uses this tile.
    public GridStat entrance;

    private void Start()
    {
        TeamManager.EnsureExists();
        if (TeamManager.Instance != null)
            TeamManager.Instance.RegisterCastle(this);
    }

    private void OnDestroy()
    {
        if (entrance != null && entrance.castle == this)
            entrance.castle = null;
        if (TeamManager.Instance != null)
            TeamManager.Instance.UnregisterCastle(this);
    }

    public override void Interact(HeroController hero)
    {
        if (hero == null)
            return;

        TeamManager.EnsureExists();

        if (hero.teamId != teamId)
        {
            teamId = hero.teamId;
            if (TeamManager.Instance != null)
                TeamManager.Instance.RegisterCastle(this);
            Debug.Log("Castle captured!");
        }
        else
        {
            Debug.Log("Opened own castle");
        }
    }
}
