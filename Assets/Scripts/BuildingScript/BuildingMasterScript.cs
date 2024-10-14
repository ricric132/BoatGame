using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingMasterScript : MonoBehaviour
{
    public BuildingObjectSO SO;
    public int x;
    public int y;
    public int z;
    public List<GameObject> sections;
    public BuildingScript.Rotation rotation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public SaveGridObject GetSaveData()
    {
        Debug.Log(SO.ID);
        SaveGridObject temp = new SaveGridObject(SO.ID, x, y, z, rotation);
        return temp;
    }
}
