using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatDetectInteractions : MonoBehaviour
{
    [SerializeField] BuildingScript buildingScript;
    [SerializeField] GridManager gridManager;
    [SerializeField] CombatController combatController;
    float detectionRange;

    [SerializeField] GameObject boatDetectCircle;
    [SerializeField] GameObject indicator;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!combatController.started)
        {
            //DetectOtherBoats();
        }
       
    }
    
    /*
    void DetectOtherBoats()
    {
        var boat = buildingScript.boatGrid.GetCircleWrap();
        List<Grid> gridsInvoved = gridManager.FindGridsInRange(boat.Key, boat.Value);
        indicator.transform.position = new Vector3(boat.Key.x, 1f, boat.Key.y);
        indicator.transform.localScale = 2 * boat.Value * Vector3.one;
        if(gridsInvoved.Count > 1)
        {
            combatController.SetUpCombat();
        }
    }
    */
}
