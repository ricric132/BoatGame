using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CombatController : MonoBehaviour
{
    List<CombatUnit> turnOrder = new List<CombatUnit>();
    HashSet<CombatUnit> Units = new HashSet<CombatUnit>();
    
    List<CombatUnit> controllable = new List<CombatUnit>();
    

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
    [SerializeField] CanvasManager mainCanvas;
    [SerializeField] UITest UITest;

    [SerializeField] ArcIndicator arcIndicator;

    [SerializeField] TeamManager teamManager;

    [SerializeField] PlayerUnitsManager playerUnitsManager;

    [SerializeField] GameObject bombFX;

    public enum CombatPhases
    {
        Planning,
        Action,
        Sim
    }

    public enum DamageType
    {
        Piercing,
        Explosive,
        Blunt
    }


    AttackType attackType = AttackType.None;
    public enum AttackType
    {
        None,
        Direct,
        Lob,
        Melee
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
            return;
        }

        if(currentPhase == CombatPhases.Sim)
        {
            return;
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
                        StartCoroutine(StartMovement(hit.collider.gameObject.GetComponent<MoveIndicatorScript>().Coords));
                    }
                }

                if (hit.collider.gameObject.TryGetComponent(out ITargetable hovered))
                {
                    UI.ShowHoverPopupInfo(hovered.GetHoverPopupInfo());
                }
                else
                {
                    UI.HideHoverPopup();
                }

                if (UI.GetPhase() != CombatUIManager.CombatUIPhase.AttackSelect)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        CombatUnit unit;
                        if (hit.collider.gameObject.TryGetComponent<CombatUnit>(out unit))
                        {
                            if (Units.Contains(unit) && !unit.dead)
                            {
                                if (IsControllable(unit))
                                {
                                    selectTurnTaker(unit);
                                }
                                else
                                {
                                    Debug.Log("not controllable");
                                }
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

                            StartCoroutine(StartDirectAttack(aimData, hitSpot));
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
                    arcIndicator.SetUp(currentTurnTaker.gameObject.transform.position, buildings.GetWorldPositionCentre(buildings.GetXYZ(hit.point + hit.normal * 0.01f)) - new Vector3(0, 0.5f, 0), currentTurnTaker.stats.STR, selectedAttack.weight, selectedAttack.aoe);

                    if(selectedAttack.aoe > 0)
                    {
                        Collider[] colliders = Physics.OverlapSphere(buildings.GetWorldPositionCentre(buildings.GetXYZ(hit.point + hit.normal * 0.01f)) - new Vector3(0, 0.5f, 0), selectedAttack.aoe/2);
                        HashSet<GameObject> tempSet = new HashSet<GameObject>();
                        foreach (Collider collider in colliders)
                        {
                            tempSet.Add(collider.gameObject);
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

                    if (Input.GetMouseButtonDown(0))
                    {
                        StartCoroutine(StartLobAttack(buildings.GetWorldPositionCentre(buildings.GetXYZ(hit.point + hit.normal * 0.01f)) - new Vector3(0, 0.5f, 0), selectedAttack));
                        return;
                    }

                }
                else if(attackType == AttackType.Melee)
                {
                    if (Input.GetMouseButtonDown(0) && hit.collider.gameObject.TryGetComponent(out ITargetable targetable))
                    {
                        Vector3Int v = targetable.GetLocation().coords - currentTurnTaker.coords;

                        if(Mathf.Abs(v.x) <= selectedAttack.maxRange &&  Mathf.Abs(v.y) <= selectedAttack.maxRange && Mathf.Abs(v.z) <= selectedAttack.maxRange)
                        {
                            StartCoroutine(StartMeleeAttack(targetable.GetGameObject(), selectedAttack));
                        }
                        
                    }
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

    HitSpot GetLowestPen(DirectAimData target, GameObject targetGO, Vector3Int? prelimCoords = null)
    {
        Vector3 coords; 
        if(prelimCoords == null)
        {
            coords = currentTurnTaker.coords;
        }
        else
        {
            coords = prelimCoords.Value;
        }


        HitSpot bestHitSpot = target.allHittableSpots[0];
        int minPierce = int.MaxValue;
        foreach (HitSpot hitspot in target.allHittableSpots)
        {
            Vector3 SourceToTarget = hitspot.transform.position - buildings.GetWorldPositionCentre(coords);
            Ray toTarget = new Ray(buildings.GetWorldPositionCentre(coords), SourceToTarget.normalized);

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

    IEnumerator StartDirectAttack(DirectAimData target, HitSpot aimedSpot)
    {
        currentPhase = CombatPhases.Action;
        ClearIndicators();
        ClearHighlighted();
        UI.SetUI(CombatUIManager.CombatUIPhase.None, currentTurnTaker);

        attackType = AttackType.None;

        yield return currentTurnTaker.DirectAttack(selectedAttack, target, aimedSpot);

        currentPhase = CombatPhases.Planning;
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, currentTurnTaker);

    }

    IEnumerator StartMovement(Vector3Int coords)
    {
        currentPhase = CombatPhases.Action;
        UI.SetUI(CombatUIManager.CombatUIPhase.None, currentTurnTaker);
        currentTurnTaker.SetPath(pathfinder.GetPath(currentTurnTaker.coords, coords));
        ClearIndicators();
        
        yield return currentTurnTaker.Move();
        
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, currentTurnTaker);
        currentPhase = CombatPhases.Planning;
        SetUnitPositions();
    }

    IEnumerator StartLobAttack(Vector3 Target, AttackAbilitySO attack)
    {
        currentPhase = CombatPhases.Action;
        Vector3[] points = arcIndicator.GetArcPoints();
        UI.SetUI(CombatUIManager.CombatUIPhase.None, currentTurnTaker);
        attackType = AttackType.None;
        arcIndicator.Toggle(false);
        ClearIndicators();
        ClearHighlighted();
        yield return currentTurnTaker.LobAttack(points, attack);

        //do effect

        if (attack.aoe > 0)
        {
            Debug.Log("boom");
            Collider[] colliders = Physics.OverlapSphere(Target, attack.aoe / 2);
            Debug.Log(colliders.Length);

            Instantiate(bombFX, Target, Quaternion.identity);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.TryGetComponent(out ITargetable highlightable))
                {
                    highlightable.TakeDamage(attack.baseDamage, DamageType.Explosive, currentTurnTaker, Target);
                }
            }
        }

        currentPhase = CombatPhases.Planning;
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, currentTurnTaker);
    }

    IEnumerator StartMeleeAttack(GameObject target, AttackAbilitySO attack)
    {
        currentPhase = CombatPhases.Action;
        ClearIndicators();
        ClearHighlighted();
        UI.SetUI(CombatUIManager.CombatUIPhase.None, currentTurnTaker);


        yield return currentTurnTaker.MeleeAttack(target, attack);

        selectedAttack = null;
        attackType = AttackType.None;

        currentPhase = CombatPhases.Planning;
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, currentTurnTaker);
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
        UI.SetUI(CombatUIManager.CombatUIPhase.ActionSelect, unit);
        currentTurnTaker = unit;
    }

    public void SelectMovement()
    {
        SetUnitPositions();
        if (currentTurnTaker.remainingMovement == 0)
        {
            Debug.Log("out of movement");
            return;
        }
        ClearIndicators();
        movableNodes = GetMovableTiles();
        foreach (PathfindingNode node in movableNodes)
        {
            //Debug.Log("placingTile :" + node.coords);
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
        //Debug.Log("coords : " + currentTurnTaker.coords);
        return pathfinder.NodeWithinRangeAdj(currentTurnTaker.coords, currentTurnTaker.remainingMovement); 
    }

    public void AddUnit(CombatUnit unit)
    {
        Units.Add(unit);
        turnOrder.Add(unit);
    }

    void ClearIndicators()
    {
        directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(false);
        foreach (GameObject indicator in moveIndicators)
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
        mainCanvas.UpdateState(CanvasManager.CanvasState.Combat);
        SetUnitPositions();
        foreach (CombatUnit unit in Units)
        {
            unit.TickTurn();
        }

        controllable.Clear();
        foreach(CharacterInfo info in playerUnitsManager.selectedTeam)
        {
            if(info != null)
            {
                controllable.Add(info.gameObject.GetComponent<CombatUnit>());
            }
        }

        UI.SetupSideBar(controllable);
    }

    void AimDirectAt()
    {
        
    }

    public void SetupAttackPanel()
    {
        if(currentTurnTaker.actionsLeft <= 0)
        {
            Debug.Log("No more actions");
            return;
        }
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
            if(GO == null)
            {
                continue;
            }
            if (GO.TryGetComponent(out ITargetable highlightable))
            {
                highlightable.WillHit(false);
            }
        }

        highlightedObjects = new HashSet<GameObject>();
    }

    public void EndTurn()
    {
        currentPhase = CombatPhases.Sim;
        UI.SetUI(CombatUIManager.CombatUIPhase.None);
        StartCoroutine(SimNPCs());

    }

    IEnumerator SimNPCs()
    {
        Debug.Log("sim started");
        yield return new WaitForSeconds(2);
       
        foreach(CombatUnit unit in turnOrder)
        {
            if(unit == null || unit.dead)
            {
                continue;
            }
            if (IsControllableUnit(unit) == false)
            {
                currentTurnTaker = unit;
                yield return CalculateAction();
                Debug.Log("sim " + unit.name + " done");
                yield return new WaitForEndOfFrame();
            }
        }
        

        Debug.Log("sim done");
        ResetUnits();
        currentPhase = CombatPhases.Planning;
    }

    bool IsControllableUnit(CombatUnit checkUnit)
    {
        foreach (CombatUnit unit in controllable)
        {
            if (unit == checkUnit)
            {
                return true;
            }
        }

        return false;
    }

    
    IEnumerator CalculateAction()
    {
        List<PathfindingNode> movable = GetMovableTiles();

        List<CombatUnit> hostiles = new List<CombatUnit>();

        foreach(CombatUnit unit in Units)
        {
            if(teamManager.CheckRelations(currentTurnTaker.stats, unit.stats) == RelationState.Agro)
            {
                hostiles.Add(unit);
            }
        }


        List<SimAction> possibleActions = new List<SimAction>();

        foreach (PathfindingNode tile in movable)
        {
            foreach (CombatUnit unit in hostiles)
            {
                if (unit.dead)
                {
                    continue;
                }
                DirectAimData aimData = unit.GetLocation();
                HitSpot hitSpot = GetLowestPen(aimData, unit.gameObject, tile.coords);

                if (hitSpot.totalPierceNeeded < 2)
                {
                    possibleActions.Add(new SimAction(hitSpot, aimData, tile.coords));

                }
                
            }
        }

        Debug.Log("actions found : " + possibleActions.Count());
        if(possibleActions.Count() > 0)
        {
            yield return StartMovement(possibleActions[0].movePoint);
            Debug.Log(possibleActions[0].movePoint);
            Debug.Log("pierce needed : " + possibleActions[0].hitSpot.totalPierceNeeded);
            yield return StartDirectAttack(possibleActions[0].aimData, possibleActions[0].hitSpot);
        }
    }
    

    void ResetUnits()
    {
        foreach(CombatUnit unit in Units)
        {
            unit.TickTurn();
        }
    }

    bool IsControllable(CombatUnit unit)
    {
        foreach(CombatUnit u in controllable)
        {
            if(u == unit)
            {
                return true;
            }
        }

        return false;
    }

    public void UnitDie(CombatUnit unit)
    {
        Units.Remove(unit);
        turnOrder.Remove(unit);


    }

    public void UpdateUnitHP(CombatUnit unit, float newHP)
    {
        for(int i = 0; i < controllable.Count; i++)
        {
            if (controllable[i] == unit)
            {
                UI.UpdateSidebarHP(i, newHP);
            }
        }
    }
}

public class SimAction
{
    public float quality;
    public HitSpot hitSpot;
    public DirectAimData aimData;
    public Vector3Int movePoint; 

    public SimAction(HitSpot hitSpot, DirectAimData aimData, Vector3Int movePoint)
    {
        this.hitSpot = hitSpot;
        this.aimData = aimData;
        this.movePoint = movePoint;
    }

}

