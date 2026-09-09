using UnityEngine;

public class HeroSelectionManager : MonoBehaviour
{
    public static HeroSelectionManager Instance;

    public HeroController selectedHero;

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

        Debug.Log("Selected hero: " + hero.name);
    }

    public void MoveSelectedHero(GridStat target)
    {
        if (selectedHero == null)
            return;

        selectedHero.MoveTo(target);
    }
}