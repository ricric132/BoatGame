using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public List<Team> allTeams;

    [SerializeField] List<CharacterInfo> testTeam1;
    [SerializeField] List<CharacterInfo> testTeam2;

    // Start is called before the first frame update
    void Start()
    {
        Team temp = CreateTeam("test team 1");
        foreach(CharacterInfo character in testTeam1)
        {
            temp.AddMember(character);
        }

        temp = CreateTeam("test team 2");
        foreach (CharacterInfo character in testTeam2)
        {
            temp.AddMember(character);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public Team CreateTeam(string Name)
    {
        Team temp = new Team();
        temp.teamName = Name;
        allTeams.Add(temp);
        return temp;
    }


    public RelationState CheckRelations(CharacterInfo source, CharacterInfo target)
    {
        if(source == target)
        {
            return RelationState.Friendly;
        }

        if (source.playerTeam)
        {
            return target.team.playerRelation;
        }

        if (target.playerTeam)
        {
            return source.team.playerRelation;
        }

        Team team1 = source.team;
        Team team2 = target.team;

        foreach(TeamRelation relation in team1.teamRelations) { 
            if(relation.subject == team2)
            {
                return relation.relationState;
            }
        }

        return RelationState.Neutral;
    }
}

[System.Serializable]
public class Team
{
    public string teamName;
    public List<CharacterInfo> members;
    public List<TeamRelation> teamRelations;
    public RelationState playerRelation;

    public Team()
    {
        teamName = "";
        members = new List<CharacterInfo>();
        teamRelations = new List<TeamRelation>();
        playerRelation = RelationState.Agro;
    }

    public void AddMember(CharacterInfo character)
    {
        character.team = this;
        members.Add(character);
    }
}

public class TeamRelation
{
    public Team haver;
    public Team subject;
    public RelationState relationState;
}

public class MemberRelation
{


}

public enum RelationState
{
    Friendly,
    Neutral,
    Agro
}
