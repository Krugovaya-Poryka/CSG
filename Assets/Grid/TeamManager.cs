using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    public List<Team> teams = new List<Team>();

    private void Awake()
    {
        Instance = this;
    }

    public Team GetTeam(int id)
    {
        return teams.Find(team => team.id == id);
    }
}