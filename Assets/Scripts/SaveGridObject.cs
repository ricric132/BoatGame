using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveGridObject 
{
    public int buildingID;
    public int x;
    public int y;
    public int z;
    public BuildingScript.Rotation rot;
    public Serializable3DArray<SectionData> sectionData;
    //add script data later

    public SaveGridObject(int id, int x, int y, int z, BuildingScript.Rotation rot)
    {
        buildingID = id;
        this.x = x;
        this.y = y;
        this.z = z;
        this.rot = rot;
    }
}

[System.Serializable]
public class SectionData
{
    public int health;
    //add status and shit later
}