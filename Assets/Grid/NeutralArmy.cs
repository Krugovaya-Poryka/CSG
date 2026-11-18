using UnityEngine;

public class NeutralArmy : MapObject
{
    public override void Interact(HeroController hero)
    {
        if (hero == null)
            return;

        // The overworld has no battle handoff yet; do not start an unconfigured battle.
        Debug.Log("NeutralArmy.Interact: Neutral army encountered (battle handoff not configured)", this);
    }
}
