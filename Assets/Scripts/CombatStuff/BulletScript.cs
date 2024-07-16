using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public int pierce = 1;
    public int damage = 5;
    public

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public IEnumerator Fire(HitSpot target, GameObject source)
    {
        Debug.Log("bullet");
        Vector3 direction = (target.transform.position - transform.position).normalized;
        int availablePierce = 1;
        HashSet<GameObject> hitObjects = new HashSet<GameObject>(); 
        while (availablePierce > 0)
        {
            transform.position += direction * 5 * Time.deltaTime;
            Collider[] hit = Physics.OverlapSphere(transform.position, 0.01f);
            foreach (Collider col in hit)
            {
                if(col.gameObject.TryGetComponent(out ITargetable intercept) && col.gameObject != source)
                {
                    availablePierce -= intercept.GetLocation().pierceNeeded;
                    intercept.TakeDamage(damage, CombatController.DamageType.Piercing, source.GetComponent<CombatUnit>(), transform.position);


                    if(availablePierce <= 0)
                    {
                        Destroy(gameObject);
                    }
                }
            }
            yield return new WaitForEndOfFrame();
        }

        yield return null;
    }
}
