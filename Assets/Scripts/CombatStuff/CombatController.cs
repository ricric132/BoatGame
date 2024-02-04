using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    Queue<CombatUnit> turnOrder = new Queue<CombatUnit>();

    HashSet<CombatUnit> Units = new HashSet<CombatUnit>();

    CombatUnit currentTurnTaker = null;
    [SerializeField] AStarPathfinding pathfinder;
    [SerializeField] BuildingScript buildings;
    List<PathfindingNode> movableNodes;

    [SerializeField] GameObject moveIndicatorPrefab;
    List<GameObject> moveIndicators = new List<GameObject>();

    bool started = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyUp(KeyCode.T))
        {
            if (!started)
            {
                SetUpCombat();
                started = true;
            }
            TickTurn();
        }
    }

    void SetUnitPositions()
    {
        foreach(CombatUnit unit in Units)
        {
            unit.StartCombatState();
        }
    }

    void TickTurn()
    {
        SetUnitPositions();
        movableNodes = GetMovableTiles();
        foreach(PathfindingNode node in movableNodes)
        {
            Debug.Log("placingTile :" + node.coords);
            GameObject indicator = Instantiate(moveIndicatorPrefab, buildings.GetWorldPosition(node.coords), Quaternion.identity);
            moveIndicators.Add(indicator);
        }
        Debug.Log("turnOver");
        NextTurn();
    }

    List<PathfindingNode> GetMovableTiles()
    {
        Debug.Log("coords : " + currentTurnTaker.coords);
        return pathfinder.NodeWithinRange(currentTurnTaker.coords, currentTurnTaker.moveRange); 
    }

    void NextTurn()
    {
        CombatUnit playedTurn = turnOrder.Dequeue();
        turnOrder.Enqueue(playedTurn);
        currentTurnTaker = turnOrder.Peek();
    }

    public void AddUnit(CombatUnit unit)
    {
        Units.Add(unit);
    }

    void SetUpCombat()
    {
        turnOrder.Clear();
        foreach(CombatUnit unit in Units)
        {
            turnOrder.Enqueue(unit);
        }
        currentTurnTaker = turnOrder.Peek();
    }
}
