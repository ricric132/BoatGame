using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public int moveRange;

    public Vector3Int coords = new Vector3Int();
    [SerializeField] BuildingScript buildingScript;
    [SerializeField] CombatController combatController;
    List<PathfindingNode> currentPath;
    Vector3 nextWaypoint;
    int waypointNum;

    [SerializeField] LineRenderer pathIndicator;

    public bool InAction; 
    
    // Start is called before the first frame update
    void Start()
    {
        combatController.AddUnit(this);
        StartCombatState();
    }

    // Update is called once per frame
    void Update()
    {

        if (currentPath != null && combatController.currentPhase == CombatController.CombatPhases.Action)
        {
            InAction = true;

            Vector3[] points = new Vector3[currentPath.Count - waypointNum + 1];
            points[0] = transform.position;
            for (int i = 0; i < currentPath.Count-waypointNum; i++)
            {
                points[i+1] = buildingScript.GetWorldPositionCentre(currentPath[i+waypointNum].coords);
            }
            pathIndicator.SetVertexCount(currentPath.Count - waypointNum + 1);
            pathIndicator.SetPositions(points);

            if (Vector3.Distance(transform.position, nextWaypoint) < 0.1)
            {
                waypointNum += 1;
                if (waypointNum > currentPath.Count - 1)
                {
                    currentPath = null;
                    InAction = false;
                    Debug.Log("arrived");
                }
                else
                {
                    nextWaypoint = buildingScript.GetWorldPositionCentre(currentPath[waypointNum].coords);
                }
            }
            transform.position += (nextWaypoint - transform.position).normalized * 2.5f * Time.deltaTime;
        }
    }

    public async void SetPath(List<PathfindingNode> path)
    {
        waypointNum = 0;
        currentPath = path;
        Vector3[] points = new Vector3[path.Count];
        for(int i = 0; i < path.Count; i++)
        {
            points[i] = buildingScript.GetWorldPositionCentre(path[i].coords);
        }
        pathIndicator.SetVertexCount(path.Count);
        pathIndicator.SetPositions(points);
    }

    public void StartCombatState()
    {
        coords = buildingScript.GetXYZ(transform.position);
    }
}
