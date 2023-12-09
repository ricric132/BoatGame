using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using DataStructures.PriorityQueue;

[Serializable]
public class Task{    
    public GameObject building;
    public BuildingTaskInfo buildingInfo;
    public PriorityLevel priority;
    public TaskType taskType;

    public Task(PriorityLevel prio, TaskType action, GameObject build){
        priority = prio;
        taskType = action;
        building = build;

        buildingInfo = build.GetComponent<BuildingTaskInfo>();
    }
    public Task()
    {
        priority = PriorityLevel.None;
        taskType = TaskType.Retrieve;
    }

}

public enum TaskType{
    Retrieve = 0,
    Deposit = 1,
    Operate = 2,
    Toggle = 3
}

public enum PriorityLevel
{
    None = -1,
    LowPrio = 0,
    MedPrio = 1,
    HighPrio = 2,
    Emergency = 4,
    FollowUp = 5

}

public class PeopleTaskManager : MonoBehaviour
{
    //List<IndividualController> unassignedPeople = new List<IndividualController>(); 
    public PriorityQueue<Task, int> unassignedTasks = new PriorityQueue<Task, int>(0);
    public Dictionary<IndividualController, Task> PeopleToTasks = new Dictionary<IndividualController, Task>();

    public List<FishingSpotScript> allFishingSpots;

    public Dictionary<Vector3Int, StorageScript> allStorageSpots = new Dictionary<Vector3Int, StorageScript>();

    List<TaskType> taskTypeList = new List<TaskType>() { TaskType.Retrieve, TaskType.Deposit, TaskType.Operate  /*, TaskType.Toggle*/};

    [SerializeField] Transform test1;
    [SerializeField] Transform test2;
    [SerializeField] AStarPathfinding pathfinding;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        unassignedTasks = new PriorityQueue<Task, int>(0);

        foreach (FishingSpotScript fishingSpot in allFishingSpots)
        {
            AssignTask(fishingSpot);
        }


        List<IndividualController> keys = new List<IndividualController>(PeopleToTasks.Keys);
        while (unassignedTasks.Size() > 0)
        {
            Task task = unassignedTasks.Top();
            unassignedTasks.Pop();
            //Debug.Log(task.taskType + " ==== " + task.priority);
            foreach (IndividualController key in keys)
            {
                //Adjust for bias later
                if (PeopleToTasks[key] == null)
                {
                    PeopleToTasks[key] = task;
                }
            }
        }
    }

    void AssignTask(AssignableBuildings building) {
        Task taskVersion = new Task(PriorityLevel.None, TaskType.Retrieve, building.gameObject);

        foreach (TaskType taskType in taskTypeList)
        {
            if(building.taskPrios[taskType] > taskVersion.priority)
            {
                taskVersion.priority = building.taskPrios[taskType];
                taskVersion.taskType = taskType;
            }
        }

        foreach (TaskType taskType in taskTypeList)
        {
            if(taskType != taskVersion.taskType && building.taskPrios[taskType] > PriorityLevel.None)
            {
                unassignedTasks.Insert(new Task(building.taskPrios[taskType], taskType, building.gameObject), 5 - (int)building.taskPrios[taskType]);
            }
        }


        if (taskVersion.priority == PriorityLevel.None)
        {
            return;
        }

        if (building.assignedIndividual == null)
        {
            unassignedTasks.Insert(taskVersion, 5 - (int)taskVersion.priority);
            return;
        }

        if (PeopleToTasks[building.assignedIndividual] == null)
        {
            PeopleToTasks[building.assignedIndividual] = taskVersion;
            return;
        }
        else if (taskVersion.priority > PeopleToTasks[building.assignedIndividual].priority)
        {
            PeopleToTasks[building.assignedIndividual] = taskVersion;
            return;
        }

        unassignedTasks.Insert(taskVersion, 5 - (int)taskVersion.priority);
    }
        
    

    public GameObject FindStorageToStore(float amount, Vector3Int startLocation) {
        HashSet<Vector3Int> storagesToCheck = new HashSet<Vector3Int>();
        foreach (KeyValuePair<Vector3Int, StorageScript> storage in allStorageSpots)
        {
            if (storage.Value.checkEnoughStorage(amount))
            {
                storagesToCheck.Add(storage.Key);
            }
        }
        return allStorageSpots[pathfinding.FindClosest(startLocation, storagesToCheck)].gameObject;
    }

    public void AddPersonToDict(IndividualController person)
    {
        PeopleToTasks[person] = null;
    }

    
}
