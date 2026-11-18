using System.Collections.Generic;
using UnityEngine;

// Manual only: use a separate test scene because Interact applies real gameplay effects.
public class LR3PolymorphismDemo : MonoBehaviour
{
    public HeroController hero;
    public List<MapObject> objects = new List<MapObject>();

    [ContextMenu("LR3/Run polymorphism demo (Play Mode)")]
    public void RunDemo()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("LR3: Enter Play Mode before running the demo.", this);
            return;
        }
        if (hero == null || objects == null || objects.Count == 0)
        {
            Debug.LogWarning("LR3: Assign a hero and map objects in the Inspector.", this);
            return;
        }

        TeamManager.EnsureExists();
        TeamManager.Instance.RegisterHero(hero);

        foreach (MapObject obj in objects)
        {
            if (obj == null)
                continue;

            Debug.Log($"LR3: MapObject -> {obj.GetType().Name}.Interact", obj);
            obj.Interact(hero);
        }
    }
}
