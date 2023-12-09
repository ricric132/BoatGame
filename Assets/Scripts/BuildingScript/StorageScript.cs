using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageScript : MonoBehaviour
{
    public float maxStorage;
    public Dictionary<ResourceSO, float> storedResources;
    public Dictionary<ResourceSO, float> toBeStored;
    public Vector3Int gridPos;

    // Start is called before the first frame update
    void Start()
    {
        maxStorage = 1000;
        storedResources = new Dictionary<ResourceSO, float>();
        toBeStored = new Dictionary<ResourceSO, float>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool checkEnoughStorage(float toStore, out float storageLeft)
    {
        float currentStored = 0;

        foreach(KeyValuePair<ResourceSO, float> resource in storedResources)
        {
            currentStored += resource.Value;
        }

        foreach (KeyValuePair<ResourceSO, float> resource  in toBeStored)
        {
            currentStored += resource.Value;
        }

        storageLeft = maxStorage - currentStored;

        return (currentStored + toStore <= maxStorage);
    }

    public bool checkEnoughStorage(float toStore)
    {
        float currentStored = 0;

        foreach (KeyValuePair<ResourceSO, float> resource in storedResources)
        {
            currentStored += resource.Value;
        }

        foreach (KeyValuePair<ResourceSO, float> resource in toBeStored)
        {
            currentStored += resource.Value;
        }

        return (currentStored + toStore <= maxStorage);
    }

    public void TransferResources(IndividualController person)
    {
        List<ResourceSO> keys = new List<ResourceSO>(person.heldItems.Keys);

        foreach(ResourceSO key in keys)
        { 
            person.heldItems[key] = 0;
            if (storedResources.ContainsKey(key))
            {
                storedResources[key] += person.heldItems[key];
            }
            else
            {
                storedResources[key] = person.heldItems[key];
            }
        }
    }
}
