using UnityEngine;

public abstract class MapObject : MonoBehaviour
{
    public Vector2Int gridPosition;

    public abstract void Interact(HeroController hero);
}