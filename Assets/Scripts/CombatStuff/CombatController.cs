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

    [SerializeField] Camera cam;

    bool started = false;

    public CombatPhases currentPhase = CombatPhases.Planning;
    public enum CombatPhases
    {
        Planning,
        Action
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPhase == CombatPhases.Action)
        {
            bool inAction = false;
            foreach(CombatUnit unit in Units)
            {
                if (unit.InAction == true)
                {
                    inAction = true;
                    break;
                }
            }

            if(inAction == false)
            {
                currentPhase = CombatPhases.Planning;
                SetUnitPositions();
            }
        }

        if (currentPhase == CombatPhases.Planning)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (moveIndicators.Contains(hit.collider.gameObject))
                {
                    hit.collider.gameObject.GetComponent<MoveIndicatorScript>().hovered = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        currentTurnTaker.SetPath(pathfinder.GetPath(currentTurnTaker.coords, hit.collider.gameObject.GetComponent<MoveIndicatorScript>().Coords));
                        ClearIndicators();
                        //TickTurn();
                    }
                }


                CombatUnit unit;
                if (hit.collider.gameObject.TryGetComponent<CombatUnit>(out unit))
                {
                    if (Units.Contains(unit))
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            selectTurnTaker(unit);
                        }
                    }
                }
            }

            if (Input.GetKeyUp(KeyCode.T))
            {
                if (!started)
                {
                    SetUpCombat();
                    started = true;
                }
                TickTurn();
            }
        }
    }

    public void StartAction()
    {
        ClearIndicators();
        currentPhase = CombatPhases.Action;
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
        ClearIndicators();
        NextTurn();
        SetUnitPositions();
        movableNodes = GetMovableTiles();
        foreach(PathfindingNode node in movableNodes)
        {
            Debug.Log("placingTile :" + node.coords);
            GameObject indicator = Instantiate(moveIndicatorPrefab, buildings.GetWorldPosition(node.coords), Quaternion.identity);
            indicator.GetComponent<MoveIndicatorScript>().Coords = node.coords;
            indicator.GetComponent<MoveIndicatorScript>().combatController = this;
            moveIndicators.Add(indicator);
        }

        Debug.Log("turnOver");
    }

    void selectTurnTaker(CombatUnit unit)
    {
        currentTurnTaker = unit;
        selectMovement();
    }

    void selectMovement()
    {
        ClearIndicators();
        movableNodes = GetMovableTiles();
        foreach (PathfindingNode node in movableNodes)
        {
            Debug.Log("placingTile :" + node.coords);
            GameObject indicator = Instantiate(moveIndicatorPrefab, buildings.GetWorldPosition(node.coords), Quaternion.identity);
            indicator.GetComponent<MoveIndicatorScript>().Coords = node.coords;
            indicator.GetComponent<MoveIndicatorScript>().combatController = this;
            moveIndicators.Add(indicator);
        }
    }

    List<PathfindingNode> GetMovableTiles()
    {
        Debug.Log("coords : " + currentTurnTaker.coords);
        return pathfinder.NodeWithinRangeAdj(currentTurnTaker.coords, currentTurnTaker.moveRange); 
    }

    void NextTurn()
    {
        currentTurnTaker = turnOrder.Peek();
        CombatUnit playedTurn = turnOrder.Dequeue();
        turnOrder.Enqueue(playedTurn);
    }

    public void AddUnit(CombatUnit unit)
    {
        Units.Add(unit);
    }

    void ClearIndicators()
    {
        foreach(GameObject indicator in moveIndicators)
        {
            Destroy(indicator);
        }
        moveIndicators.Clear(); 
    }

    void SetUpCombat()
    {
        turnOrder.Clear();
        foreach(CombatUnit unit in Units)
        {
            turnOrder.Enqueue(unit);
        }
        currentTurnTaker = turnOrder.Peek();
        CombatUnit playedTurn = turnOrder.Dequeue();
        turnOrder.Enqueue(playedTurn);
    }
}
