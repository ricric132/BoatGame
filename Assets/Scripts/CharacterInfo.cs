using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CharacterInfo : MonoBehaviour
{
    public string unitName;
    public string unitDesc;
    public Image unitImage;
    public int STR;
    public int SPD;
    public int DEX;
    public int INT;
    public int VIT;

    public Team team;
    public bool playerTeam;
    public bool controllable;

    public CombatUnit combat;
    

    // Start is called before the first frame update
    void Start()
    {
        combat = GetComponent<CombatUnit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
