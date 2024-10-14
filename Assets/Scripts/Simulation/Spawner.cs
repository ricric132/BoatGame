using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    float totalCooldown = 30;
    float timeTillNext = 0;

    float maxDist = 200;
    float minDist = 50;

    [SerializeField] GameObject toSpawn;
    [SerializeField] Transform playerBoat;
    [SerializeField] CombatController combatController;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if(combatController.started)
        {
            return;
        }

        timeTillNext -= Time.deltaTime;

        if(timeTillNext < 0)
        {
            timeTillNext = totalCooldown;

            Vector3 randomDir = new Vector3(Random.Range(-10,10), 0, Random.Range(-10,10));
            randomDir.Normalize();
            float randomDist = Random.Range(minDist, maxDist);


            Instantiate(toSpawn, playerBoat.transform.position + randomDir * randomDist, Quaternion.identity);
            
        }
    }
}
