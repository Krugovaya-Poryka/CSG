using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    public List<Team> teams = new List<Team>();

    private void Awake()
    {
        Instance = this;
        if (teams == null)
            teams = new List<Team>();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void EnsureExists()
    {
        if (Instance == null)
            Instance = FindAnyObjectByType<TeamManager>();
        if (Instance == null)
            new GameObject("TeamManager").AddComponent<TeamManager>();
    }

    public Team GetTeam(int id)
    {
        return teams?.Find(team => team != null && team.id == id);
    }

    private Team GetOrCreateTeam(int id)
    {
        if (teams == null)
            teams = new List<Team>();
        Team team = GetTeam(id);
        if (team == null)
        {
            team = new Team { id = id, teamName = "Team " + id };
            teams.Add(team);
        }
        return team;
    }

    public void RegisterHero(HeroController hero)
    {
        if (hero == null)
            return;
        UnregisterHero(hero);
        Team team = GetOrCreateTeam(hero.teamId);
        if (team.heroes == null)
            team.heroes = new List<HeroController>();
        team.heroes.Add(hero);
    }

    public void UnregisterHero(HeroController hero)
    {
        if (teams == null || hero == null)
            return;
        foreach (Team team in teams)
            if (team != null && team.heroes != null)
                team.heroes.RemoveAll(member => member == hero);
    }

    public void RegisterCastle(Castle castle)
    {
        if (castle == null)
            return;
        UnregisterCastle(castle);
        Team team = GetOrCreateTeam(castle.teamId);
        if (team.castles == null)
            team.castles = new List<Castle>();
        team.castles.Add(castle);
    }

    public void UnregisterCastle(Castle castle)
    {
        if (teams == null || castle == null)
            return;
        foreach (Team team in teams)
            if (team != null && team.castles != null)
                team.castles.RemoveAll(member => member == castle);
    }
}
