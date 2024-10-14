using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class IndividualController : MonoBehaviour
{
    [SerializeField] AStarPathfinding pathfinding;
    List<PathfindingNode> path;
    [SerializeField] BuildingScript buildingScript;
    [SerializeField] GridManager gridManager;

    public Grid occupiedGrid;
    Vector3 nextWaypoint;
    int waypointNum;

    public Task mainTask;
    //public List<Task> taskSequence;
    //Task currentTask;


    bool HasTask = false;

    public CharacterInfo info;

    public PeopleTaskManager personManager;

    public Dictionary<ResourceSO, float> heldItems = new Dictionary<ResourceSO, float>();
    public float maxHeldItems = 10;

    public PriorityLevel storeHeldItemPriority;


    // Start is called before the first frame update
    void Awake()
    {
        personManager = FindObjectOfType<PeopleTaskManager>();
        pathfinding = FindObjectOfType<AStarPathfinding>();
        buildingScript = FindObjectOfType<BuildingScript>();
        gridManager = FindObjectOfType<GridManager>();

        personManager.AddPersonToDict(this);
        info = GetComponent<CharacterInfo>();   
    }

    // Update is called once per frame
    void Update()
    {
        if (!HasTask)
        {
            CheckTask();
        }
        
        if(Input.GetKeyDown(KeyCode.Q)){
            
        }   

        if(path != null){
            if(Vector3.Distance(transform.position, nextWaypoint) < 0.1){
                waypointNum += 1;
                if(waypointNum > path.Count - 1){
                    path = null;
                    Debug.Log("arrived");
                    StartCoroutine(DoTask(mainTask));
                    //arrived at target
                }
                else{
                    nextWaypoint = gridManager.GetWorldPositionCentre(path[waypointNum].coords, occupiedGrid);
                }
            }
            transform.position += (nextWaypoint - transform.position).normalized * 1f * Time.deltaTime;
        }

        
    }

    void CheckTask()
    {
        Task selectedTask  = new Task();
        if (personManager.PeopleToTasks[this] != null)
        {
            HasTask = true;
            selectedTask = personManager.PeopleToTasks[this];
            Debug.Log("FOUND TASK");
        }
        PriorityLevel unpackPrio = UpdateUnpackBagPrio();
        storeHeldItemPriority = unpackPrio;


        if (unpackPrio > selectedTask.priority)
        {
            selectedTask = new Task(unpackPrio, TaskType.Deposit, personManager.FindStorageToStore(CheckBagTotal(), gridManager.GetXYZ(transform.position, gridManager.combatGrid.wholeGrid)));
        }


        if(selectedTask.priority != PriorityLevel.None)
        {
            SetTask(selectedTask);
        }
    }

    PriorityLevel UpdateUnpackBagPrio()
    {
        float fillPercent = CheckBagPercent();

        if (fillPercent == 0)
        {
            return PriorityLevel.None;
        }
        else if (fillPercent <= 0.25)
        {
            return PriorityLevel.LowPrio;
        }
        else if (fillPercent <= 0.5)
        {
            return PriorityLevel.MedPrio;
        }
        else if (fillPercent <= 0.75)
        {
            return PriorityLevel.HighPrio;
        }
        else
        {
            return PriorityLevel.FollowUp;
        }
    }

    float CheckBagPercent()
    {
        return CheckBagTotal()/maxHeldItems;
    }

    public float CheckBagTotal()
    {
        float total = 0;
        foreach (ResourceSO key in heldItems.Keys)
        {
            total += heldItems[key];
        }
        return total;
    }

    public void SetTask(Task task){
        mainTask = task;
        SetPath(task.buildingInfo.gridPos);
    }

    public void SetPath(Vector3 Location){
        path = pathfinding.GetPath(gridManager.GetXYZ(transform.position, gridManager.combatGrid.wholeGrid), gridManager.GetXYZ(Location, occupiedGrid), occupiedGrid);
        if(path == null){
            return;
        }
        nextWaypoint = gridManager.GetWorldPositionCentre(path[0].coords, occupiedGrid);
        waypointNum = 0;
    }

    public void SetPath(Vector3Int Coords)
    {
        path = pathfinding.GetPath(gridManager.GetXYZ(transform.position, occupiedGrid), Coords, occupiedGrid);
        if (path == null)
        {
            return;
        }
        nextWaypoint = gridManager.GetWorldPositionCentre(path[0].coords, occupiedGrid);
        waypointNum = 0;
    }

    IEnumerator DoTask(Task task)
    {
        //Do something
        yield return new WaitForSeconds(3);
        if(task.building.tag == "FishingSpot")
        {
            if(task.taskType == TaskType.Operate)
            {
                task.building.GetComponent<FishingSpotScript>().OperateFishingSpot(this);
            }
            else if(task.taskType == TaskType.Retrieve)
            {
                task.building.GetComponent<FishingSpotScript>().RetrieveResource(this);
            }
        }

        if(task.building.tag == "StorageSpot")
        {
            if (task.taskType == TaskType.Deposit)
            {
                task.building.GetComponent<StorageScript>().TransferResources(this);
            }
            else if (task.taskType == TaskType.Retrieve)
            {
                task.building.GetComponent<StorageScript>().TransferResources(this);
            }
        }

        HasTask = false;
        personManager.PeopleToTasks[this] = null;
    }
}
