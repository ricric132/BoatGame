using LlockhamIndustries.ExtensionMethods;
using LlockhamIndustries.Misc;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngineInternal;
using static UnityEngine.EventSystems.EventTrigger;

public class NPCBoat : MonoBehaviour
{
    public float detectionRadius;
    public GridManager gridManager;
    Grid playerBoat;

    public GridHolder ownGrid;

    public Transform boatCentre;

    public CombatController combatController;

    float turnSpeed = 60;
    float speed = 5;

    public List<CharacterInfo> boatMembers;

    public TeamManager teamManager;

    Rigidbody rb;


    // Start is called before the first frame update
    void Awake()
    {
        

        rb = GetComponent<Rigidbody>();
        
        gridManager = FindObjectOfType<GridManager>();
        combatController = FindObjectOfType<CombatController>();
        teamManager = FindObjectOfType<TeamManager>();

        playerBoat = gridManager.GetPlayerBoat();
        ownGrid.SetUp();
        gridManager.LoadFromJson(ownGrid.selectedPrefab.jsonName, ownGrid.grid);

        List<Vector3> initPos = gridManager.GetRandomWalkableCoords(boatMembers.Count, ownGrid.grid);   

        for(int i = 0; i < boatMembers.Count; i++)
        {
            boatMembers[i].transform.position = gridManager.GetWorldPositionCentre(initPos[i], ownGrid.grid);
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (combatController.started)
        {
            rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            return;
        }

        rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;

        if (gridManager.buildingScript.boatGrid == null)
        {
            return;
        }

        playerBoat = gridManager.GetPlayerBoat();
        var playerCircle = playerBoat.GetCircleWrap();
        var ownCircle = ownGrid.grid.GetCircleWrap();

        Vector3 COM = GetComponent<Rigidbody>().centerOfMass + transform.position;
        boatCentre.position = COM;


        if(Vector2.Distance(playerCircle.Key, ownCircle.Key) < detectionRadius + playerCircle.Value + ownCircle.Value)
        {
            Vector2 pointDir = new Vector2(boatCentre.forward.x, boatCentre.forward.z);
            float toRot = Vector2.SignedAngle(pointDir, playerCircle.Key - new Vector2(boatCentre.position.x, boatCentre.position.z));

            

            if(toRot == 0) 
            { 

            }
            else if(toRot > 0)
            {
                boatCentre.Rotate(0, Mathf.Min(-turnSpeed * Time.deltaTime, -toRot), 0);
            }
            else
            {
                boatCentre.Rotate(0, Mathf.Max(turnSpeed * Time.deltaTime, toRot), 0);
            }

            rb.velocity = speed * new Vector3(boatCentre.forward.x, 0,  boatCentre.forward.z);
        }


   
    }
}
