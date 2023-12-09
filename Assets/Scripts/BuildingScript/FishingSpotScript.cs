using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingSpotScript : AssignableBuildings
{
    [SerializeField] ResourceSO fish;

    Dictionary<ResourceSO, float> standardInput;
    Dictionary<ResourceSO, float> standardOutput;

    // Start is called before the first frame update
    void Start()
    {
        standardInput = new Dictionary<ResourceSO, float> ();
        standardOutput = new Dictionary<ResourceSO, float> { { fish, 2 } };

        outputMaxStorage = new Dictionary<ResourceSO, float> { { fish, 10 } };
        outputStorage = new Dictionary<ResourceSO, float> { { fish, 0 } };


        inputMaxStorage = new Dictionary<ResourceSO, float>();
        inputStorage = new Dictionary<ResourceSO, float>();
        inputMin = new Dictionary<ResourceSO, float>();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log(defaultPrio + 1);
            UpdateTaskTypePriority();
            List<TaskType> keys = new List<TaskType>(taskPrios.Keys);
            foreach(TaskType key in keys)
            {
                Debug.Log(key + ":  " + taskPrios[key]);
            }
        }
    }

    public void OperateFishingSpot(IndividualController personOperating) //add person argument for stat and trait checks and exp gain
    {
        ResourceInResourceOut(standardInput, standardOutput, 0f);
        Debug.Log("fish: " + outputStorage[fish]);
    }
}
