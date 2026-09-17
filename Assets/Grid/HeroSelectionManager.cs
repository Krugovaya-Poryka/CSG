using UnityEngine;

public class HeroSelectionManager : MonoBehaviour
{
    public GridStat selectedTarget;
    public static HeroSelectionManager Instance;

    public HeroController selectedHero;

    public HeroUI heroUI;

    private void Awake()
    {
        Instance = this;
    }

    public void ClickTile(GridStat target)
    {
        if (selectedHero == null)
            return;

        if (selectedTarget == target)
        {
            selectedHero.ConfirmMove();
            selectedTarget = null;
            return;
        }

        selectedTarget = target;
        selectedHero.PreviewPath(target);
    }
    
    public void SelectHero(HeroController hero)
    {
        if (selectedHero != null)
            selectedHero.selected = false;

        selectedHero = hero;

        if (selectedHero != null)
            selectedHero.selected = true;

        if (heroUI != null)
            heroUI.ShowHero(selectedHero);

        Debug.Log("Selected hero: " + hero.name);
    }

    public void MoveSelectedHero(GridStat target)
    {
        if (selectedHero == null)
            return;

        ClickTile(target);
    }
    

    
    public void EndTurn()
    {
        HeroController[] heroes = FindObjectsByType<HeroController>(FindObjectsSortMode.None);

        foreach (HeroController hero in heroes)
        {
            hero.ResetMovement();
        }
    }
}