using UnityEngine;

// Attached only to the Grid hero prefab for the LR2 demonstration.
[RequireComponent(typeof(HeroController))]
public class HeroInventoryDemo : MonoBehaviour
{
    private bool initialized;
    private HeroController hero;
    private Item demoSword;

    public bool CanRemoveSword => hero != null && hero.inventory != null &&
        hero.inventory.items != null && demoSword != null && hero.inventory.items.Contains(demoSword);

    private void Start()
    {
        AddDemoItemsOnce();
    }

    [ContextMenu("LR2/Add demo items once")]
    public void AddDemoItemsOnce()
    {
        if (!Application.isPlaying || initialized)
            return;

        hero = GetComponent<HeroController>();
        if (hero == null || hero.inventory == null)
            return;

        demoSword = new Item { itemName = "Training sword", slot = EquipmentSlot.Weapon, attackBonus = 1 };
        hero.inventory.AddItem(demoSword);
        hero.inventory.AddItem(new Item { itemName = "Wooden shield", slot = EquipmentSlot.Shield, defenseBonus = 1 });
        initialized = true;
    }

    [ContextMenu("LR2/Remove demo sword")]
    public void RemoveDemoSword()
    {
        if (Application.isPlaying && CanRemoveSword)
            hero.inventory.RemoveItem(demoSword);
    }
}
