using UnityEngine;

public class HeroSelectionManager : MonoBehaviour
{
    public static HeroSelectionManager Instance;

    [Tooltip("Team whose movement is restored by EndTurn. The current map starts with team 0.")]
    public int currentTeamId;

    public HeroController selectedHero;

    public HeroUI heroUI;

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ClickTile(GridStat target)
    {
        if (selectedHero == null)
            return;

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

        Debug.Log(hero != null ? "Selected hero: " + hero.name : "No hero selected");
    }
    
    

    
    public void EndTurn()
    {
        Team team = TeamManager.Instance != null
            ? TeamManager.Instance.GetTeam(currentTeamId) : null;
        if (team == null || team.heroes == null)
            return;

        foreach (HeroController hero in team.heroes)
        {
            if (hero != null)
                hero.ResetMovement();
        }
    }
}
