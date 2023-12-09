using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using TMPro;

public class PlayerResources : MonoBehaviour
{
    [SerializeField] ResourceSO[] allResourceSos;
    public Dictionary<ResourceSO, int> resources = new Dictionary<ResourceSO, int>{};
    [SerializeField] List<GameObject> resourceDisplayObjects;
    [SerializeField] Transform resourceDisplayList;


    // Start is called before the first frame update
    void Start()
    {
        foreach(ResourceSO resource in allResourceSos){
            resources.Add(resource, 1000);
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateResourceTexts();
    }

    void UpdateResourceTexts(){
        List<ResourceSO> toShow = new List<ResourceSO>(resources.Keys);
        foreach(Transform displayObject in resourceDisplayList){
            if(toShow.Contains(displayObject.GetComponent<ResourcesDisplayObject>().resourceSO)){
                displayObject.gameObject.SetActive(true);
            }
            else{
                displayObject.gameObject.SetActive(true);  
            }
        }
    }

    public int GetResourceAmount(ResourceSO resource){
        return resources[resource];
    }

    public void ChangeResourceAmount(ResourceSO resource, int amount){
        resources[resource] += amount;
    }

    
}
