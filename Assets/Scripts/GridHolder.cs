using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GridHolder : MonoBehaviour
{
    public PrefabCreationSO selectedPrefab;

    public Grid grid = new Grid();

    // Start is called before the first frame update
    void Awake()
    {        

        
    }

    public void SetUp()
    {
        grid = new Grid((int)selectedPrefab.gridDimensions.x, (int)selectedPrefab.gridDimensions.y, (int)selectedPrefab.gridDimensions.z, transform.position, transform.rotation.eulerAngles.y);
        grid.parentTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        grid.origin = transform.position;
    }

    public void PrepareForLoad()
    {
        GameObject toBeDeleted = new GameObject();


        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).SetParent(toBeDeleted.transform);
        }

        Destroy(toBeDeleted);


    }
}
