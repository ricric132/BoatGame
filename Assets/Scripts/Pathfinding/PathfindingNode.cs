using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class PathfindingNode
{
    public float distance;
    public float rootDistance;
    public float manhattenDistance;
    public Serializable3DArray<bool> enterableSides;
    public Serializable3DArray<bool> walls;
    public bool visited;
    public PathfindingNode previousNode;
    public Vector3Int coords;

    public bool hasFloor;
    public PathfindingNode(int x, int y, int z)
    {
        hasFloor = false;
        enterableSides = new Serializable3DArray<bool>(3, 3, 3);
        walls = new Serializable3DArray<bool>(3, 3, 3);
        this.coords = new Vector3Int(x, y, z);
    }

    public bool Equals(PathfindingNode obj)
    {
        if(hasFloor != obj.hasFloor)
        {
            return false;
        }


        if (!enterableSides.Equals(obj.enterableSides))
        {
            return false;
        }


        if (!walls.Equals(obj.walls))
        {
            return false;
        }

        return true;
    }

    public void UpdateNode(float newDistance, PathfindingNode prevNode)
    {
        if (rootDistance < newDistance)
        {
            return;
        }

        rootDistance = newDistance;
        distance = rootDistance + manhattenDistance;
        previousNode = prevNode;
    }

    public void UpdateNode(PathfindingNode prevNode)
    {
        previousNode = prevNode;
    }

    public void ChangeRootDist(float newDist)
    {
        rootDistance = Mathf.Min(newDist, rootDistance);
    }
}
