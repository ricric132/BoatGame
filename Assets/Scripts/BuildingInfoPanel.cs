using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingInfoPanel : MonoBehaviour
{
    [SerializeField] Image buildingIcon;
    [SerializeField] Image workerIcon;
    [SerializeField] TextMeshProUGUI workerInfo;
    [SerializeField] TextMeshProUGUI workerName;
    [SerializeField] TextMeshProUGUI buildingName;
    [SerializeField] TextMeshProUGUI buildingDescription;

    public void Setup(Image buildingIcon, string buildingName, string buildingDesc, string workerName, string workerInfo, Image workerIcon){
        this.buildingName.text = buildingName;
    }

    
    public void Setup(Image buildingIcon, string buildingName, string buildingDesc){
        this.buildingName.text = buildingName;
    }
}
