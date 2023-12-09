using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResourcesDisplayObject : MonoBehaviour
{
    public ResourceSO resourceSO;
    [SerializeField] PlayerResources playerResources;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] TextMeshProUGUI amountNum;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateText();
    }
    public void UpdateText(){
        label.text = resourceSO.name;
        amountNum.text = playerResources.resources[resourceSO].ToString();
    }
}
