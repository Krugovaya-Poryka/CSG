using UnityEngine;

public class Castle : TeamOwnedObject
{
    public override void Interact(HeroController hero)
    {
        if (hero.teamId != teamId)
        {
            teamId = hero.teamId;
            Debug.Log("Castle captured!");
        }
        else
        {
            Debug.Log("Opened own castle");
        }
    }
}