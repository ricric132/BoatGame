using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject resourcesTab;
    [SerializeField] GameObject buildingTabs;
    [SerializeField] GameObject defaultTabs;
    [SerializeField] GameObject buildingInfoTab;
    [SerializeField] GameObject unitManagerTab;
    [SerializeField] GameObject combatUI;
    [SerializeField] GameObject camToggle;


    [SerializeField] BuildingInfoPanel buildingInfoPanelSetup;
    [SerializeField] Camera cam;


    public CanvasState currentState;
    public CanvasState prevState;

    //CanvasState[] currentState;
    AssignableBuildings selectedBuilding;
    [SerializeField] GameObject gridIndicator;

    [SerializeField] GameObject ESCPopup;

    [SerializeField] GameObject boatNavCanvas;

    [SerializeField] TextMeshProUGUI cantBuildText;

    [SerializeField] GameObject battleWinPopup;
    [SerializeField] GameObject battleLosePopup;

    [SerializeField] GameObject movementInstructions;

    [SerializeField] TextMeshProUGUI anchorText;



    public enum CanvasState{
        None,
        CityManagement,
        ResourcesTab,
        BuildingMode,
        UnitManager,
        Combat,
        BoatControl
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateState(CanvasState.CityManagement);

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleEscapeWindow();
        }

        if (currentState == CanvasState.BoatControl)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.F)){
            ToggleResourceTab();
        }

        if(Input.GetKeyDown(KeyCode.G)){
            ToggleBuildingTab();
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            int layerMask = 1 << 8;
            RaycastHit hit; 

            if(Physics.Raycast(ray, out hit, 100f, layerMask)){
                if(hit.collider != null){
                    if(!buildingInfoTab.activeSelf){
                        if(hit.collider.tag == "FishingSpot"){
                            BuildingObjectSO SO = hit.collider.gameObject.GetComponent<FishingSpotScript>().buildingObjectSO;
                            if(hit.collider.gameObject.GetComponent<FishingSpotScript>().assignedIndividual != null){
                                OpenBuildingWindow(SO.icon, SO.buildingName, SO.buildingDescription, hit.collider.gameObject.GetComponent<FishingSpotScript>().assignedIndividual);
                            }
                            else{
                                OpenBuildingWindow(SO.icon, SO.buildingName, SO.buildingDescription);
                            }
                        }
                    }
                    else{
                        buildingInfoTab.SetActive(false);
                    }
                }
                else{
                    buildingInfoTab.SetActive(false);
                }
            }
        }



    }

    public void ToggleResourceTab(){
        if(resourcesTab.activeSelf){
            UpdateState(CanvasState.CityManagement);
        }
        else{
            UpdateState(CanvasState.ResourcesTab);
        }
    }
    public void ToggleBuildingTab(){
        if(buildingTabs.activeSelf){
            UpdateState(CanvasState.CityManagement);
        }
        else{
            UpdateState(CanvasState.BuildingMode);
        }
    }

    public void ToggleUnitManagerTab()
    {
        if (unitManagerTab.activeSelf)
        {
            UpdateState(CanvasState.CityManagement);
        }
        else
        {
            UpdateState(CanvasState.UnitManager);
        }
    }

    public void ToggleBoatControl(bool on)
    {
        if(on)
        {
            prevState = currentState;
            UpdateState(CanvasState.BoatControl);
        }
        else
        {
            Debug.Log(prevState);
            UpdateState(prevState);
        }
    }

    public void ToggleEscapeWindow()
    {
        ESCPopup.SetActive(!ESCPopup.activeSelf);
    }

    public void UpdateState(CanvasState newState){
        currentState = newState;

        //defaultTabs.SetActive(false);
        resourcesTab.SetActive(false);
        buildingTabs.SetActive(false);
        gridIndicator.SetActive(false);
        unitManagerTab.SetActive(false);
        combatUI.SetActive(false);
        defaultTabs.SetActive(false);
        boatNavCanvas.SetActive(false);
        camToggle.SetActive(false);
        movementInstructions.SetActive(false);

        switch(currentState){
            case CanvasState.None:
                break;
            case CanvasState.CityManagement:
                defaultTabs.SetActive(true);
                camToggle.SetActive(true);
                movementInstructions.SetActive(true);

                break;
            case CanvasState.ResourcesTab:
                resourcesTab.SetActive(true);
                defaultTabs.SetActive(true);
                camToggle.SetActive(true);
                movementInstructions.SetActive(true);


                break;
            case CanvasState.BuildingMode:
                buildingTabs.SetActive(true);
                gridIndicator.SetActive(true);
                defaultTabs.SetActive(true);
                camToggle.SetActive(true);
                movementInstructions.SetActive(true);

                break;
            case CanvasState.UnitManager:
                unitManagerTab.SetActive(true);
                defaultTabs.SetActive(true);
                camToggle.SetActive(true);
                movementInstructions.SetActive(true);


                break;
            case CanvasState.Combat:
                combatUI.SetActive(true);
                movementInstructions.SetActive(true);
                break;
            case CanvasState.BoatControl:
                boatNavCanvas.SetActive(true);
                camToggle.SetActive(true);
                break;

        }

    }


    public void OpenBuildingWindow(Image icon, String name, String description, IndividualController worker = null){
        if(currentState != CanvasState.CityManagement){
            return;
        }

        
        buildingInfoTab.SetActive(true);

        if(worker == null){
            buildingInfoPanelSetup.Setup(icon, name, description);
        }
        else{
            buildingInfoPanelSetup.Setup(icon, name, description, worker.info.unitName, worker.info.unitDesc, worker.info.unitImage);
        }
    }

    public void DisplayCantPlaceReason(string reason)
    {

    }

    public void DisplayWinPopup(int amount)
    {
        battleWinPopup.GetComponent<BattleWinPopup>().PopUp(amount);
    }

    public void DisplayLosePopup()
    {
        battleLosePopup.GetComponent<BattleLosePopup>().PopUp();
    }


    public void UpdateAnchor(bool down)
    {
        if (down)
        {
            anchorText.text = "Anchor : Down";
        }
        else 
        { 
            anchorText.text = "Anchor : Up";
        }
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
