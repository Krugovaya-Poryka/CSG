using UnityEngine;
using UnityEngine.InputSystem;

// Created at runtime by HeroController, so no scene or Canvas setup is required.
[DefaultExecutionOrder(-50)]
public class HeroDetailsPanel : MonoBehaviour
{
    private static HeroDetailsPanel instance;
    private bool isOpen;
    private Vector2 scrollPosition;

    private static Rect PanelRect => new Rect(16, 16,
        Mathf.Min(440, Screen.width - 32), Mathf.Min(600, Screen.height - 32));

    public static void EnsureExists()
    {
        if (instance == null)
            instance = FindAnyObjectByType<HeroDetailsPanel>();
        if (instance == null)
            new GameObject("Hero Details Panel").AddComponent<HeroDetailsPanel>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        // The scene contains an unfinished, full-screen hero UI with placeholder text.
        // Hide only that known layout at runtime; use this panel's H toggle instead.
        GameObject placeholderPanel = GameObject.Find("HeroPanel");
        if (placeholderPanel != null &&
            placeholderPanel.transform.Find("NameText") != null &&
            placeholderPanel.transform.Find("StatsText") != null &&
            placeholderPanel.transform.Find("ArmyText") != null &&
            placeholderPanel.transform.Find("InventoryText") != null)
        {
            placeholderPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
            isOpen = !isOpen;
    }

    // MouseController uses screen coordinates with the origin at the bottom left.
    public static bool BlocksMapInput(Vector2 screenPosition)
    {
        return instance != null && instance.isActiveAndEnabled && instance.isOpen &&
            PanelRect.Contains(new Vector2(screenPosition.x, Screen.height - screenPosition.y));
    }

    private void OnGUI()
    {
        if (!isOpen)
            return;

        GUILayout.BeginArea(PanelRect, "Hero — H to close", GUI.skin.window);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        HeroController hero = HeroSelectionManager.Instance != null
            ? HeroSelectionManager.Instance.selectedHero : null;

        if (hero == null)
        {
            GUILayout.Label("No hero selected");
        }
        else
        {
            GUILayout.Label(hero.name);
            GUILayout.Label("Team: " + hero.teamId);
            GUILayout.Label("Movement: " + hero.movementPoints + " / " + hero.maxMovementPoints);
            GUILayout.Space(10);
            GUILayout.Label("STATS");
            if (hero.stats != null)
            {
                GUILayout.Label("Attack: " + hero.Attack + " (equipment +" + (hero.inventory?.AttackBonus ?? 0) + ")");
                GUILayout.Label("Defense: " + hero.Defense + " (equipment +" + (hero.inventory?.DefenseBonus ?? 0) + ")");
                GUILayout.Label("Power: " + hero.stats.power);
                GUILayout.Label("Knowledge: " + hero.stats.knowledge);
            }

            GUILayout.Space(10);
            GUILayout.Label("EQUIPMENT");
            DrawEquipmentSlot(hero.inventory, EquipmentSlot.Weapon, "Weapon");
            DrawEquipmentSlot(hero.inventory, EquipmentSlot.Shield, "Shield");
            DrawEquipmentSlot(hero.inventory, EquipmentSlot.Helmet, "Helmet");
            DrawEquipmentSlot(hero.inventory, EquipmentSlot.Armor, "Armor");

            GUILayout.Space(10);
            GUILayout.Label("INVENTORY / BACKPACK");
            if (hero.inventory == null || hero.inventory.items == null || hero.inventory.items.Count == 0)
                GUILayout.Label("Empty");
            else
            {
                // Apply changes after drawing, so collection/layout iteration stays stable.
                Item toEquip = null;
                foreach (Item item in hero.inventory.items)
                    if (item != null)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(ItemText(item));
                        if (item.slot != EquipmentSlot.None && GUILayout.Button("Equip", GUILayout.Width(70)))
                            toEquip = item;
                        GUILayout.EndHorizontal();
                    }
                if (toEquip != null)
                    hero.inventory.EquipItem(toEquip);
            }

            HeroInventoryDemo demo = hero.GetComponent<HeroInventoryDemo>();
            if (demo != null && demo.CanRemoveSword && GUILayout.Button("LR2: remove training sword"))
                demo.RemoveDemoSword();

            GUILayout.Space(10);
            GUILayout.Label("ARMY");
            if (hero.army == null)
                GUILayout.Label("No army assigned");
            int slotCount = hero.army != null && hero.army.slots != null
                ? Mathf.Max(7, hero.army.slots.Length) : 7;
            for (int i = 0; i < slotCount; i++)
                GUILayout.Label((i + 1) + ". " + GridHeroArmyView.GetSlotText(hero.army, i));
        }

        GUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private static void DrawEquipmentSlot(HeroInventory inventory, EquipmentSlot slot, string label)
    {
        Item item = inventory?.GetEquippedItem(slot);
        GUILayout.BeginHorizontal();
        GUILayout.Label(label + ": " + (item != null ? ItemText(item) : "Empty"));
        bool unequip = item != null && GUILayout.Button("Unequip", GUILayout.Width(70));
        GUILayout.EndHorizontal();
        if (unequip)
            inventory.UnequipItem(item);
    }

    private static string ItemText(Item item)
    {
        return item.itemName + " [ATK +" + item.attackBonus + ", DEF +" + item.defenseBonus + "]";
    }
}
