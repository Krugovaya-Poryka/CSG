using UnityEngine;

public class Mine : TeamOwnedObject
{
    public override void Interact(HeroController hero)
    {
        Debug.Log("Mine interaction");
    }
}