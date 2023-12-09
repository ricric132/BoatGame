using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class AssignableBuildings : MonoBehaviour
{
    public IndividualController assignedIndividual;
    public IndividualController workingIndividual;

    public PriorityLevel defaultPrio = PriorityLevel.MedPrio;
    public Dictionary<TaskType, PriorityLevel> taskPrios = new Dictionary<TaskType, PriorityLevel>{ 
        { TaskType.Retrieve, PriorityLevel.None },
        { TaskType.Deposit, PriorityLevel.MedPrio },
        { TaskType.Operate, PriorityLevel.MedPrio },
        { TaskType.Toggle, PriorityLevel.MedPrio }
    };

    public BuildingObjectSO buildingObjectSO;
    

    public Dictionary<ResourceSO, float> inputMaxStorage;
    public Dictionary<ResourceSO, float> inputStorage;
    public Dictionary<ResourceSO, float> inputMin;

    public Dictionary<ResourceSO, float> outputMaxStorage;
    public Dictionary<ResourceSO, float> outputStorage;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        UpdateTaskTypePriority();

    }

    public void ResourceInResourceOut(Dictionary<ResourceSO, float> used, Dictionary<ResourceSO, float> produced, float time)
    {
        foreach (KeyValuePair<ResourceSO, float> pair in used)
        {
            inputStorage[pair.Key] -= pair.Value;
        }

        foreach (KeyValuePair<ResourceSO, float> pair in produced)
        {
            outputStorage[pair.Key] += pair.Value;
        }
    }

    public void RetrieveResource(IndividualController retriever)
    {
        List<ResourceSO> keys = new List<ResourceSO>(outputStorage.Keys);
        foreach (ResourceSO key in keys) {
            float spaceLeft = retriever.maxHeldItems - retriever.CheckBagTotal();
            if (spaceLeft >= outputStorage[key])
            {
                if (retriever.heldItems.Keys.Contains(key))
                {
                    retriever.heldItems[key] += outputStorage[key];
                }
                else
                {
                    retriever.heldItems[key] = outputStorage[key];
                }
                outputStorage[key] = 0;
            }
            else
            {
                if (retriever.heldItems.Keys.Contains(key))
                {
                    retriever.heldItems[key] += spaceLeft;
                }
                else
                {
                    retriever.heldItems[key] = spaceLeft;
                }
                outputStorage[key] -= spaceLeft;
            }
        }
    }

    public void UpdateTaskTypePriority()
    {

        taskPrios[TaskType.Retrieve] = PriorityLevel.None;
        taskPrios[TaskType.Deposit] = PriorityLevel.None;
        taskPrios[TaskType.Operate] = defaultPrio;
        taskPrios[TaskType.Toggle] = defaultPrio;


        List<ResourceSO> outputKeys = new List<ResourceSO>(outputMaxStorage.Keys);
        foreach (ResourceSO key in outputKeys)
        {
            if (outputStorage[key] >= outputMaxStorage[key] && taskPrios[TaskType.Retrieve] < defaultPrio+1)
            {
                taskPrios[TaskType.Operate] = PriorityLevel.None;
                if (taskPrios[TaskType.Retrieve] < defaultPrio + 1)
                {
                    taskPrios[TaskType.Retrieve] = defaultPrio + 1;
                }
            }

            if (outputStorage[key] >= outputMaxStorage[key]*0.5 && taskPrios[TaskType.Retrieve] < defaultPrio)
            {
                taskPrios[TaskType.Retrieve] = defaultPrio;
            }

            if (outputStorage[key] > 0 && taskPrios[TaskType.Retrieve] < defaultPrio - 1)
            {
                taskPrios[TaskType.Retrieve] = defaultPrio - 1;
            }
        }

        if (inputMin.Count() > 0)
        {
            List<ResourceSO> inputKeys = new List<ResourceSO>(inputMaxStorage.Keys);
            foreach (ResourceSO key in inputKeys)
            {
                {
                    if (inputStorage[key] <= inputMaxStorage[key] * 0.25 && taskPrios[TaskType.Deposit] < defaultPrio + 1)
                    {
                        taskPrios[TaskType.Deposit] = defaultPrio + 1;
                    }

                    if (inputStorage[key] <= inputMaxStorage[key] * 0.5 && taskPrios[TaskType.Deposit] < defaultPrio)
                    {
                        taskPrios[TaskType.Deposit] = defaultPrio;
                    }

                    if (inputStorage[key] < inputMaxStorage[key] * 0.75 && taskPrios[TaskType.Deposit] < defaultPrio - 1)
                    {
                        taskPrios[TaskType.Deposit] = defaultPrio - 1;
                    }
                }
            }
        }



    }



}
