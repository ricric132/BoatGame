using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class CombatUnit : MonoBehaviour, ITargetable
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

    [SerializeField] string unitName;

    public Material baseMaterial;
    public Material hitIndicatorMaterial;
    public MeshRenderer visual;

    public DirectAimData directAimData;

    public List<AttackAbilitySO> availableAttacks;

    public int maxHP;
    public int curHP;

    public int weaponProficiency;

    [SerializeField] GameObject bulletPrefab;


    int actionIndex = 0;
    List<CombatAction> queuedActions = new List<CombatAction>();

    [SerializeField] GameObject directAimIndicator;

    public CharacterInfo stats;

   

    // Start is called before the first frame update
    void Start()
    {
        combatController.AddUnit(this);
        StartCombatState();
    }

    // Update is called once per frame
    void Update()
    {
        bool aiming = false;
        foreach(CombatAction action in queuedActions)
        {
            if (action.type == ActionType.Attack)
            {
                if (action.action.attackType == CombatController.AttackType.Direct)
                {
                    aiming = true;
                    break;   
                }
            }
        }
        if (!aiming)
        {
            directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(false);
        }

        if (currentPath != null && combatController.currentPhase == CombatController.CombatPhases.Action)
        {
            InAction = true;
        }
    }

    /*

    public IEnumerator RunActions()
    {
        Debug.Log("running" + queuedActions.Count);

        directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(false);

        actionIndex = 0;
        while(actionIndex < queuedActions.Count)
        {
            CombatAction action = queuedActions[actionIndex];
            Debug.Log(action.type);
            if (action.type == ActionType.Move)
            {
                currentPath = action.movementPath;
                yield return Move();
            }

            if(action.type == ActionType.Attack)
            {
                Debug.Log("Check");
                if(action.action.attackType == CombatController.AttackType.Direct)
                {
                    yield return DirectAttack(action);
                }
            }

            actionIndex++;
            yield return new WaitForSeconds(0.5f);
        }

        queuedActions.Clear();
    }

    IEnumerator Move()
    {
        while (currentPath != null)
        {
            Vector3[] points = new Vector3[currentPath.Count - waypointNum + 1];
            points[0] = transform.position;
            for (int i = 0; i < currentPath.Count - waypointNum; i++)
            {
                points[i + 1] = buildingScript.GetWorldPositionCentre(currentPath[i + waypointNum].coords);
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
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    IEnumerator DirectAttack(CombatAction action)
    {
        Debug.Log("shooting");
        int random = Random.Range(0, action.target.allHittableSpots.Count - 1);
        HitSpot target =  action.target.allHittableSpots[random];
        if(target != null)
        {
            BulletScript bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<BulletScript>();
            yield return bullet.Fire(target, gameObject);
        }
        yield return null;  
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

        if (queuedActions.Count > 0)
        {
            if (queuedActions[queuedActions.Count-1].type == ActionType.Move)
            {
                queuedActions[queuedActions.Count -1] = new CombatAction(path);
            }
        }
        else
        {
            queuedActions.Add(new CombatAction(path));
        }
    }



    public void QueueAction(AttackAbilitySO action, DirectAimData target, HitSpot hitSpot)
    {
       queuedActions.Add(new CombatAction(action, target));
       directAimIndicator.GetComponent<DirectAimIndicator>().Toggle(true);
       directAimIndicator.GetComponent<DirectAimIndicator>().SetLocation(transform.position, buildingScript.GetWorldPosition(target.coords), hitSpot);
       Debug.Log("queued");
    }
    */

    public void StartCombatState()
    {
        coords = buildingScript.GetXYZ(transform.position);
    }

    public string GetName()
    {
        return unitName;
    }

    public DirectAimData GetLocation()
    {
        directAimData.coords = coords;
        return directAimData;
    }

    public IEnumerator DirectAttack(AttackAbilitySO attack, DirectAimData target, HitSpot aimedSpot)
    {
        Debug.Log("shooting");
        int random = Random.Range(0, target.allHittableSpots.Count - 1);
        HitSpot spot = target.allHittableSpots[random];
        if (target != null)
        {
            BulletScript bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<BulletScript>();
            yield return bullet.Fire(spot, gameObject);
        }
        yield return null;
    }

    public void SetPath(List<PathfindingNode> path)
    {
        waypointNum = 0;
        currentPath = path;
        Vector3[] points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            points[i] = buildingScript.GetWorldPositionCentre(path[i].coords);
        }
        pathIndicator.SetVertexCount(path.Count);
        pathIndicator.SetPositions(points);

        if (queuedActions.Count > 0)
        {
            if (queuedActions[queuedActions.Count - 1].type == ActionType.Move)
            {
                queuedActions[queuedActions.Count - 1] = new CombatAction(path);
            }
        }
        else
        {
            queuedActions.Add(new CombatAction(path));
        }
    }

    public IEnumerator Move()
    {
        while (currentPath != null)
        {
            Vector3[] points = new Vector3[currentPath.Count - waypointNum + 1];
            points[0] = transform.position;
            for (int i = 0; i < currentPath.Count - waypointNum; i++)
            {
                points[i + 1] = buildingScript.GetWorldPositionCentre(currentPath[i + waypointNum].coords);
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
            yield return new WaitForEndOfFrame();
        }
        yield return null;
    }

    public void WillHit(bool hit)
    {
        if (hit)
        {
            Debug.Log("I've been hit captn");
            visual.material = hitIndicatorMaterial;
        }
        else
        {
            visual.material = baseMaterial;
        }
    }
}
