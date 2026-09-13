using UnityEngine;

public class HeroSelectionManager : MonoBehaviour
{
    public static HeroSelectionManager Instance;

    public HeroController selectedHero;

    public HeroUI heroUI;

    private void Awake()
    {
        Instance = this;
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

        selectedHero.MoveTo(target);
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