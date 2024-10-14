using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridObjPrefab", menuName = "ScriptableObjects/GridObjPrefab")]
public class PrefabCreationSO : ScriptableObject
{
    public Grid grid;
    public string jsonName;
    public List<SaveGridObject> savedObjects = new List<SaveGridObject>();

    public Vector3 gridDimensions;


    public void Reset(Vector3 dimensions)
    {
        Debug.Log("set");
        gridDimensions = dimensions;
        savedObjects.Clear();
        grid = new Grid((int)dimensions.x, (int)dimensions.y, (int)dimensions.z, Vector3.zero, 0);
    }
}
