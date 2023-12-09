using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridObject
{
    public bool occupied;
    public bool pillared;
    public BuildingObjectSO building;
    public int x;
    public int y;
    public int z;

    public GridObject(int x, int y, int z){
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
