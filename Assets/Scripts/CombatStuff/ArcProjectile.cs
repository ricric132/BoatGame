using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class ArcProjectile : MonoBehaviour
{
    float speed = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator Launch(Vector3[] points, AttackAbilitySO attack)
    {
        int counter = 0;

        while(counter < points.Length)
        {
            float moveAmount = speed * Time.deltaTime;
            Vector3 dir = (points[counter] - transform.position).normalized;
            float dist = (points[counter] - transform.position).magnitude;

            while (moveAmount > dist)
            {
                counter++;
                if(counter == points.Length)
                {
                    transform.position = points[points.Length - 1];
                    break;
                }
                dir = (points[counter] - transform.position).normalized;
                dist = (points[counter] - transform.position).magnitude;
            }

            if(counter < points.Length)
            {
                transform.position += dir * moveAmount;
            }

            yield return new WaitForEndOfFrame();
        }

        Destroy(gameObject);
    }
}
