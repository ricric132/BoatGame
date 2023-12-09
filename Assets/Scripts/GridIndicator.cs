using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridIndicator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 10; i++){
            Physics.IgnoreLayerCollision(9, i);
        }            
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
