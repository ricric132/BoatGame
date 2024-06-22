using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CombatController : MonoBehaviour
{
    List<CombatUnit> turnOrder = new List<CombatUnit>();
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

    [SerializeField] AttackOptionsPanel attackOptionsPanel;
    [SerializeField] CombatUIManager UI;
    [SerializeField] UITest UITest;

    [SerializeField] ArcIndicator arcIndicator;


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
        if (currentTurnTaker == null)
        {
            UI.SetUI(CombatUIManager.CombatUIPhase.None);
        }

        if (currentPhase == CombatPhases.Action)
        {
            bool inAction = false;
            foreach (CombatUnit unit in Units)
            {
                if (unit.InAction == true)
                {
                    inAction = true;
                    break;
                }
            }

            if (inAction == false)
            {
                currentPhase = CombatPhases.Planning;
                SetUnitPositions();
            }
        }

        if (currentPhase == CombatPhases.Planning)
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f) && UITest.IsPointerOverUIElement() == false)
            {
                if (moveIndicators.Contains(hit.collider.gameObject))
                {
                    hit.collider.gameObject.GetComponent<MoveIndicatorScript>().hovered = true;
                    if (Input.GetMouseButtonDown(0))
                    {
                        IEnumerator coroutine = (IEnumerator)StartMovement(hit.collider.gameObject.GetComponent<MoveIndicatorScript>().Coords);
                        StartCoroutine(coroutine);
                    }
                }

                if (UI.GetPhase() != CombatUIManager.CombatUIPhase.AttackSelect)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        CombatUnit unit;
                        if (hit.collider.gameObject.TryGetComponent<CombatUnit>(out unit))
                        {
                            if (Units.Contains(unit))
                            {
                                selectTurnTaker(unit);
                            }
                            else
                            {
                                currentTurnTaker = null;
                            }
                        }
                        else
                        {
                            currentTurnTaker = null;
                        }
                    }
                }

                if (attackType == AttackType.Direct && currentTurnTaker != null)
                {
                    directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(true);
                    if (hit.collider.gameObject != null && hit.collider.gameObject.TryGetComponent(out ITargetable target))
                    {
                        DirectAimData aimData = target.GetLocation();

                        HitSpot hitSpot = GetLowestPen(aimData, hit.collider.gameObject);
                     

                        if (Input.GetMouseButtonDown(0))
                        {

                            IEnumerator coroutine = (IEnumerator)StartDirectAttack(aimData, hitSpot);
                            StartCoroutine(coroutine);
                            return;
                        }

                        directAimIndicator.GetComponent<DirectAimIndicator>().SetLocation(buildings.GetWorldPositionCentre(currentTurnTaker.coords), buildings.GetWorldPosition(aimData.coords), hitSpot);

                        Vector3 SourceToTarget = hitSpot.transform.position - buildings.GetWorldPositionCentre(currentTurnTaker.coords);
                        Ray toTarget = new Ray(buildings.GetWorldPositionCentre(currentTurnTaker.coords), SourceToTarget.normalized);

                        HashSet<GameObject> tempSet = new HashSet<GameObject>();
                        RaycastHit[] penetrated = Physics.RaycastAll(toTarget, SourceToTarget.magnitude);

                        foreach (RaycastHit obj in penetrated)
                        {
                            tempSet.Add(obj.collider.gameObject);
                        }

                        foreach (GameObject GO in tempSet)
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
                else if (attackType == AttackType.Lob)
                {

                    arcIndicator.Toggle(true);
                    arcIndicator.SetUp(currentTurnTaker.gameObject.transform.position, buildings.GetWorldPositionCentre(buildings.GetXYZ(hit.point + hit.normal * 0.01f)) - new Vector3(0, 0.5f, 0), currentTurnTaker.stats.STR, selectedAttack.weight);
                }

            }
            else
            {
                directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(false);
                arcIndicator.Toggle(false);
                ClearHighlighted();
            }
            if (Input.GetKeyUp(KeyCode.T))
            {
                if (!started)
                {
                    SetUpCombat();
                    started = true;
                }
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

    IEnumerable StartDirectAttack(DirectAimData target, HitSpot aimedSpot)
    {
        currentPhase = CombatPhases.Action;
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, currentTurnTaker);
        attackType = AttackType.None;

        yield return currentTurnTaker.DirectAttack(selectedAttack, target, aimedSpot);

        currentPhase = CombatPhases.Planning;

    }


    IEnumerable StartMovement(Vector3Int coords)
    {
        currentPhase = CombatPhases.Action;
        currentTurnTaker.SetPath(pathfinder.GetPath(currentTurnTaker.coords, coords));
        ClearIndicators();

        yield return currentTurnTaker.Move();
        currentPhase = CombatPhases.Planning;

    }
    public void StartAction()
    {
        ClearIndicators();
        StartCoroutine(RunActions());
        //currentPhase = CombatPhases.Action;
    }

    IEnumerator RunActions()
    {
        currentPhase = CombatPhases.Action;

        foreach(CombatUnit unit in turnOrder)
        {
            yield return unit.RunActions();

            yield return new WaitForSeconds(0.5f);
        }

        currentPhase = CombatPhases.Planning;
        SetUnitPositions();
    }

    void SetUnitPositions()
    {
        foreach (CombatUnit unit in Units)
        {
            unit.StartCombatState();
        }
    }

    void selectTurnTaker(CombatUnit unit)
    {
        attackType = AttackType.None;
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect);
        infoPanel.Setup(unit.GetName());
        currentTurnTaker = unit;
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

    List<PathfindingNode> GetMovableTiles()
    {
        Debug.Log("coords : " + currentTurnTaker.coords);
        return pathfinder.NodeWithinRangeAdj(currentTurnTaker.coords, currentTurnTaker.moveRange); 
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
            turnOrder.Add(unit);
        }
        currentTurnTaker = turnOrder[0];
        SetUnitPositions();
    }

    void AimDirectAt()
    {
        
    }

    public void SetupAttackPanel()
    {
        Debug.Log("attack selected");
        UI.SetUI(CombatUIManager.CombatUIPhase.AttackSelect, currentTurnTaker);
    }

    public void SelectAttack(AttackAbilitySO attackAbilitySO)
    {
        selectedAttack = attackAbilitySO;
        attackType = selectedAttack.attackType;
    }

    int DistanceFrom(Vector3Int coord1, Vector3Int coord2)
    {
        return Mathf.Abs(coord1.x - coord2.x) + Mathf.Abs(coord1.y - coord2.y) + Mathf.Abs(coord1.z - coord2.z);
    }

    void ClearHighlighted()
    {
        foreach (GameObject GO in highlightedObjects)
        {
            if (GO.TryGetComponent(out ITargetable highlightable))
            {
                highlightable.WillHit(false);
            }
        }

        highlightedObjects = new HashSet<GameObject>();
    }
}
