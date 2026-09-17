using UnityEngine;

public class NeutralArmy : MapObject
{
    public override void Interact(HeroController hero)
    {
        Debug.Log("Start battle");
    }
}