using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerUnitsManager : MonoBehaviour
{
    public List<CharacterInfo> allUnits = new List<CharacterInfo>();
    public List<CharacterInfo> selectedTeam = new List<CharacterInfo>(4);

    // Start is called before the first frame update
    void Start()
    {
        /*
        selectedTeam.Add(null);
        selectedTeam.Add(null);
        selectedTeam.Add(null);
        selectedTeam.Add(null);
        */
        foreach (CharacterInfo unit in allUnits)
        {
            unit.playerTeam = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int RemoveUnit(CharacterInfo info)
    {
        for(int i = 0; i < 4; i++)
        {
            if (selectedTeam[i] == info)
            {
                selectedTeam[i] = null;
                return i;
            }
        }
        return -1;
    }

    public CharacterInfo SelectTeamUnit(int index, CharacterInfo info, int indexFrom)
    {
        CharacterInfo prev = selectedTeam[index];
        selectedTeam[index] = info;
        if (prev != null)
        {
            if(indexFrom != -1)
            {
                selectedTeam[indexFrom] = prev;
            }
        }
        return prev;
    }


}
