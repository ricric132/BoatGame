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

    [SerializeField] GameObject attackIndicatorPrefab;
    List<GameObject> attackIndicators = new List<GameObject>();

    [SerializeField] Camera cam;

    bool started = false;

    public CombatPhases currentPhase = CombatPhases.Planning;

    public GridManager gridManager;

    [SerializeField] GameObject directAimIndicator;

    HashSet<GameObject> highlightedObjects = new HashSet<GameObject>();

    AttackAbilitySO selectedAttack;

    public enum CombatPhases
    {
        Planning,
        Action
    }


    AttackType attackType = AttackType.None;
    public enum AttackType
    {
        None,
        Direct,
        Lob
    }

    public UnitInfoPanel infoPanel;

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



                if (attackType == AttackType.Direct)
                {
                    if (hit.collider.gameObject != null && hit.collider.gameObject.TryGetComponent(out ITargetable target)) {
                        DirectAimData aimData = target.GetLocation();

                        HitSpot hitSpot = GetLowestPen(aimData, hit.collider.gameObject);
                        directAimIndicator.GetComponent<DirectAimIndicator>().SetLocation(buildings.GetWorldPositionCentre(currentTurnTaker.coords), buildings.GetWorldPosition(aimData.coords), hitSpot);

                        Vector3 SourceToTarget = hitSpot.transform.position - buildings.GetWorldPositionCentre(currentTurnTaker.coords);
                        Ray toTarget = new Ray(buildings.GetWorldPositionCentre(currentTurnTaker.coords), SourceToTarget.normalized);



                        HashSet<GameObject> tempSet = new HashSet<GameObject>();
                        RaycastHit[] penetrated = Physics.RaycastAll(toTarget, SourceToTarget.magnitude);
     
                        foreach(RaycastHit obj in penetrated)
                        {
                            tempSet.Add(obj.collider.gameObject);
                        }

                        foreach(GameObject GO in tempSet)
                        {
                            if (!highlightedObjects.Contains(GO))
                            {
                                if (GO.TryGetComponent(out ITargetable highlightable))
                                {
                                    highlightable.WillHit(true);
                                }
                            }
                        }

                        foreach (GameObject GO in highlightedObjects)
                        {
                            if (!tempSet.Contains(GO))
                            {
                                if (GO.TryGetComponent(out ITargetable highlightable))
                                {
                                    highlightable.WillHit(false);
                                }
                            }
                        }

                        highlightedObjects = tempSet;

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

    HitSpot GetLowestPen(DirectAimData target, GameObject targetGO)
    {
        HitSpot bestHitSpot = target.allHittableSpots[0];
        int minPierce = int.MaxValue;
        foreach (HitSpot hitspot in target.allHittableSpots)
        {
            Vector3 SourceToTarget = hitspot.transform.position - buildings.GetWorldPositionCentre(currentTurnTaker.coords);
            Ray toTarget = new Ray(buildings.GetWorldPositionCentre(currentTurnTaker.coords), SourceToTarget.normalized);

            RaycastHit[] penetrated = Physics.RaycastAll(toTarget, SourceToTarget.magnitude);

            int totalPierce = 0;
            foreach (RaycastHit hit in penetrated)
            {
                if (hit.collider.gameObject != gameObject && hit.collider.gameObject.TryGetComponent(out ITargetable intercept))
                { 
                    totalPierce += intercept.GetLocation().pierceNeeded;
                }
            }
            hitspot.totalPierceNeeded = totalPierce;

            if (totalPierce < minPierce)
            {
                minPierce = totalPierce; 
                bestHitSpot = hitspot;
            }
        }

        return bestHitSpot;
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
        NextTurn();
        SetUnitPositions();
        SelectMovement();

        Debug.Log("turnOver");
    }

    void selectTurnTaker(CombatUnit unit)
    {
        infoPanel.Setup(unit.GetName());
        currentTurnTaker = unit;
        SelectMovement();
    }

    public void SelectMovement()
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

    /*
    public void ShowInRange()
    {
        
        ClearIndicators();
        Dictionary<Vector3Int, GridObject> inSightNodes = gridManager.GetLineOfSight(currentTurnTaker.coords, 5);

        foreach (KeyValuePair<Vector3Int, GridObject> node in inSightNodes)
        {
            GameObject indicator = Instantiate(attackIndicatorPrefab, buildings.GetWorldPosition(node.Key.x, node.Key.y, node.Key.z), Quaternion.identity);
            attackIndicators.Add(indicator);
            indicator.GetComponent<OutlineIndicatorSides>().SetSides(node.Key, inSightNodes);

        }
        
    }
    */

    public void selectRangedAttack()
    {
        attackType = AttackType.Direct;
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

        foreach (GameObject indicator in attackIndicators)
        {
            Destroy(indicator);
        }
        attackIndicators.Clear();
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

    void AimDirectAt()
    {
        
    }

    int DistanceFrom(Vector3Int coord1, Vector3Int coord2)
    {
        return Mathf.Abs(coord1.x - coord2.x) + Mathf.Abs(coord1.y - coord2.y) + Mathf.Abs(coord1.z - coord2.z);
    }
}
