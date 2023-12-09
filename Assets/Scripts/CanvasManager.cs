using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] GameObject resourcesTab;
    [SerializeField] GameObject buildingTabs;
    [SerializeField] GameObject defaultTabs;
    [SerializeField] GameObject buildingInfoTab;
    [SerializeField] BuildingInfoPanel buildingInfoPanelSetup;
    [SerializeField] Camera cam;

    public CanvasState currentState;
    //CanvasState[] currentState;
    AssignableBuildings selectedBuilding;
    [SerializeField] GameObject gridIndicator;
    

    
    public enum CanvasState{
        None,
        CityManagement,
        ResourcesTab,
        BuildingMode
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
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

    void UpdateState(CanvasState newState){
        currentState = newState;

        resourcesTab.SetActive(false);
        buildingTabs.SetActive(false);
        defaultTabs.SetActive(false);
        gridIndicator.SetActive(false);

        switch(currentState){
            case CanvasState.None:
                break;
            case CanvasState.CityManagement:
                defaultTabs.SetActive(true);
                break;
            case CanvasState.ResourcesTab:
                resourcesTab.SetActive(true);
                
                break;
            case CanvasState.BuildingMode:
                buildingTabs.SetActive(true);
                gridIndicator.SetActive(true);
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
            buildingInfoPanelSetup.Setup(icon, name, description, worker.personName, worker.personInfo, worker.personImage);
        }
    }
}
