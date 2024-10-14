using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CombatUnit : MonoBehaviour, ITargetable
{
    public Vector3Int coords = new Vector3Int();
    BuildingScript buildingScript;
    CombatController combatController;
    GridManager gridManager;
    List<PathfindingNode> currentPath;
    Vector3 nextWaypoint;
    int waypointNum;

    [SerializeField] LineRenderer pathIndicator;

    public bool InAction;

    public Material baseMaterial;
    public Material hitIndicatorMaterial;
    public MeshRenderer visual;

    public DirectAimData directAimData;

    public List<AttackAbilitySO> availableAttacks;

    public int maxHP;
    public int curHP;

    public int weaponProficiency;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] GameObject throwablePrefab;

    public CharacterInfo stats;

    public int remainingMovement;
    public int actionsLeft;
    public int maxActions = 1;

    public bool dead;

    [SerializeField] Transform HpBar;
    [SerializeField] GameObject bloodSplurt;

    [SerializeField] Animator animator;

    bool stallForAnimation = false;




    // Start is called before the first frame update
    void Awake()
    {
        buildingScript = FindObjectOfType<BuildingScript>();
        combatController = FindObjectOfType<CombatController>();
        gridManager = FindObjectOfType<GridManager>();
        //combatController.AddUnit(this);
        //StartCombatState();
        curHP = maxHP;


    }

    // Update is called once per frame
    void Update()
    {

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
        coords = gridManager.GetXYZ(transform.position, gridManager.combatGrid.wholeGrid);
    }

    public string GetName()
    {
        return stats.unitName;
    }

    public DirectAimData GetLocation()
    {
        directAimData.coords = coords;
        return directAimData;
    }

    public IEnumerator DirectAttack(AttackAbilitySO attack, DirectAimData target, HitSpot aimedSpot)
    { 
        actionsLeft--;



        int random = Random.Range(0, target.allHittableSpots.Count - 1);
        HitSpot spot = target.allHittableSpots[random];
        if (target != null)
        {
            animator.SetBool("isIdol", false);

            stallForAnimation = true;
            transform.LookAt(gridManager.GetWorldPositionCentre(target.coords, gridManager.combatGrid.wholeGrid));
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

            animator.SetTrigger("shoot");
            while (stallForAnimation)
            {
                yield return new WaitForEndOfFrame();
            }

            BulletScript bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity).GetComponent<BulletScript>();
            yield return bullet.Fire(spot, gameObject);

            animator.SetBool("isIdol", true);
        }
    }


    
    public IEnumerator LobAttack(Vector3[] points, AttackAbilitySO attack)
    {
        actionsLeft--;
        animator.SetBool("isIdol", false);
        stallForAnimation = true;

        transform.LookAt(points[points.Length-1]);
        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        animator.SetTrigger("throw");
        

        while (stallForAnimation)
        {
            yield return new WaitForEndOfFrame();
        }
       
        ArcProjectile projectile = Instantiate(throwablePrefab, transform.position, Quaternion.identity).GetComponent<ArcProjectile>();
        yield return projectile.Launch(points, attack);
        animator.SetBool("isIdol", true);
    }

    public void ResumePausedAction()
    {
        stallForAnimation = false;
    }

    public IEnumerator MeleeAttack(GameObject target,  AttackAbilitySO attack)
    {
        actionsLeft--;

        

        
        if (target.TryGetComponent(out ITargetable hittable))
        {
            animator.SetBool("isIdol", false);
            animator.SetTrigger("punch");
            yield return new WaitForSeconds(0.5f);
            hittable.TakeDamage(attack.baseDamage, attack.damageType, this);
            yield return new WaitForSeconds(0.5f);
            animator.SetBool("isIdol", true);
        }
        
        yield return null;
    }

    public void SetPath(List<PathfindingNode> path)
    {
        waypointNum = 0;
        nextWaypoint = transform.position;
        currentPath = path;
        Vector3[] points = new Vector3[path.Count];
        for (int i = 0; i < path.Count; i++)
        {
            points[i] = gridManager.GetWorldPositionCentre(path[i].coords, gridManager.combatGrid.wholeGrid);
        }
        pathIndicator.SetVertexCount(path.Count);
        pathIndicator.SetPositions(points);

        remainingMovement -= path.Count - 1;
    }

    public IEnumerator Move()
    {
        animator.SetBool("isWalking", true);
        while (currentPath != null)
        {
            Vector3[] points = new Vector3[currentPath.Count - waypointNum + 1];
            points[0] = transform.position;
            for (int i = 0; i < currentPath.Count - waypointNum; i++)
            {
                points[i + 1] = gridManager.GetWorldPositionCentre(currentPath[i + waypointNum].coords, gridManager.combatGrid.wholeGrid);
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
                    nextWaypoint = gridManager.GetWorldPositionCentre(currentPath[waypointNum].coords, gridManager.combatGrid.wholeGrid);
                }
            }
            transform.position += (nextWaypoint - transform.position).normalized * 2.5f * Time.deltaTime;
            transform.LookAt(nextWaypoint);
            transform.rotation = Quaternion.Euler(new Vector3(0, transform.rotation.eulerAngles.y, 0));

            yield return new WaitForEndOfFrame();
        }
        animator.SetBool("isWalking", false);
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

    public void TickTurn()
    {
        actionsLeft = 1;
        remainingMovement = stats.SPD;
    }

    public void TakeDamage(int damage, CombatController.DamageType damageType, CombatUnit attacker, Vector3 point = default(Vector3))
    {
        curHP -= damage;
        HpBar.localScale = new Vector3((float)curHP / (float)maxHP, 1, 1);

        combatController.UpdateUnitHP(this, curHP);

        if(damageType == CombatController.DamageType.Piercing)
        {
            Quaternion q;
            Vector3 a = Vector3.Cross(point, attacker.gameObject.transform.position);
            q.x = a.x;
            q.y = a.y;
            q.z = a.z;
            q.w = Mathf.Sqrt(Mathf.Pow(point.magnitude, 2) * Mathf.Pow(attacker.gameObject.transform.position.magnitude, 2)) + Vector3.Dot(point, attacker.gameObject.transform.position);

            Instantiate(bloodSplurt, point, q);
        }

        if (curHP <= 0 && !dead)
        {
            dead = true;
            //transform.position = transform.position - new Vector3(0, 0.5f, 0);
            //transform.rotation = Quaternion.Euler(90f, 0, 0);
            combatController.CheckBattleEnd();
            animator.SetBool("isDead", true);
            GetComponent<Collider>().enabled = false;

            //combatController.UnitDie(this);
            //Destroy(gameObject);
        }
    }

    public HoverPopupInfo GetHoverPopupInfo()
    {
        return new HoverPopupInfo(stats.unitName, maxHP, curHP);

    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
