using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public int moveRange;

    public Vector3Int coords = new Vector3Int();
    [SerializeField] BuildingScript buildingScript;
    [SerializeField] CombatController combatController;
    
    // Start is called before the first frame update
    void Start()
    {
        combatController.AddUnit(this);
        StartCombatState();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCombatState()
    {
        coords = buildingScript.GetXYZ(transform.position);
    }
}
