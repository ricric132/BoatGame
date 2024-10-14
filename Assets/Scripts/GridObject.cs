using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridObject
{
    public bool occupied;
    public bool pillared;
    public BuildingSectionSO section;

    public int x;
    public int y;
    public int z;
    public PathfindingNode pathfindingNode;
    public Grid owner;
    
    public GridObject(int x, int y, int z){
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void RefreshCoords(int newX, int newY, int newZ)
    {
        x = newX;
        y = newY; 
        z = newZ;

        pathfindingNode.coords.x = x;
        pathfindingNode.coords.y = y;
        pathfindingNode.coords.z = z;
    }
}
