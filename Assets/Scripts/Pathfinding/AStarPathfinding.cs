using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.IO.LowLevel.Unsafe;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using DataStructures.PriorityQueue;
using UnityEditor.Compilation;
using UnityEditor;
using Unity.VisualScripting;

public class AStarPathfinding : MonoBehaviour
{

    public GridManager gridManager;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void UpdateNodes(){

    }

    public List<PathfindingNode> GetPath(Vector3Int start, Vector3Int destination){
        List<PathfindingNode> path = new List<PathfindingNode>();
        if(start == destination)
        {
            path.Add(gridManager.grid.GetValue(destination.x, destination.y, destination.z).pathfindingNode);
            return path;
        }

        if(FindPath(start, destination) == false){
            Debug.Log("no Path found");
            return null;
        }

        PathfindingNode prevStep = gridManager.grid.GetValue(destination.x, destination.y, destination.z).pathfindingNode;
        Debug.Log("Node = " + prevStep.coords);
        
        while(prevStep != null){
            path.Add(prevStep);
            prevStep = prevStep.previousNode;
            
        }
       
        path.Reverse();
        return path;
    }
    

    public bool FindPath(Vector3Int start, Vector3Int destination){
        Debug.Log("start:  " + start);
        for (int x = 0; x < gridManager.grid.x; x++){
            for(int y = 0; y < gridManager.grid.y; y++){
                for(int z = 0; z < gridManager.grid.z; z++){
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.manhattenDistance = Mathf.Abs(destination.x - x) + Mathf.Abs(destination.y - y) + Mathf.Abs(destination.z - z);
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.rootDistance = Mathf.Infinity;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.distance = Mathf.Infinity;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.visited = false;
                }
            }
        }

        PriorityQueue<PathfindingNode, float> nodesToVisit = new PriorityQueue<PathfindingNode, float>(0);
        gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode.UpdateNode(0f, null);
        nodesToVisit.Insert(gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode, 0);
        
        while(nodesToVisit.Size() > 0){
            Vector3Int currentNode = nodesToVisit.Pop().coords;
            gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode.visited = true;



            for(int x = -1; x < 2; x++){
                for(int y = -1; y < 2; y++){
                    for(int z = -1; z < 2; z++){  
                        if(currentNode.x + x > gridManager.grid.x -1 || currentNode.z + z > gridManager.grid.z - 1|| currentNode.y + y > gridManager.grid.y - 1|| currentNode.x + x < 0 || currentNode.z + z < 0 || currentNode.y + y < 0){
                            continue;
                        }
                        if(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.visited){
                            continue;
                        }
                        if(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.enterableSides[-x+1, -y+1, -z+1] == false){
                            continue;
                        }
                        if(gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode.enterableSides[x+1, y+1, z+1] == false){
                            continue;
                        }
                        
                        gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.UpdateNode(gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode.rootDistance + 1, gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode);

                        if(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.coords == destination){
                            return true;
                        }
                        
                        nodesToVisit.Insert(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode, gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.distance);
                    }
                }
            } 
        }

        return false;
    }

    public Vector3Int FindClosest(Vector3Int start, HashSet<Vector3Int> destination)
    {
        Debug.Log("start:  " + start);
        if(destination.Count == 0)
        {
            Debug.Log("no storages");
            return new Vector3Int(-1, -1, -1);
        }

        if (destination.Contains(start))
        {
            gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode.UpdateNode(null);
            return start;
        }

        for (int x = 0; x < gridManager.grid.x; x++)
        {
            for (int y = 0; y < gridManager.grid.y; y++)
            {
                for (int z = 0; z < gridManager.grid.z; z++)
                {
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.rootDistance = Mathf.Infinity;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.visited = false;
                }
            }
        }

        Queue<PathfindingNode> nodesToVisit = new Queue<PathfindingNode>();
        gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode.UpdateNode(0f, null);
        nodesToVisit.Enqueue(gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode);

        while (nodesToVisit.Count() > 0)
        {
            Vector3Int currentNode = nodesToVisit.Dequeue().coords;
            gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode.visited = true;



            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if (currentNode.x + x > gridManager.grid.x - 1 || currentNode.z + z > gridManager.grid.z - 1 || currentNode.y + y > gridManager.grid.y - 1 || currentNode.x + x < 0 || currentNode.z + z < 0 || currentNode.y + y < 0)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.visited)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.enterableSides[-x + 1, -y + 1, -z + 1] == false)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.enterableSides[x + 1, y + 1, z + 1] == false)
                        {
                            continue;
                        }

                        gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.UpdateNode(gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode.rootDistance + 1, gridManager.grid.GetValue(currentNode.x, currentNode.y, currentNode.z).pathfindingNode);

                        if (destination.Contains(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.coords))
                        {
                            return gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode.coords;
                        }

                        nodesToVisit.Enqueue(gridManager.grid.GetValue(currentNode.x + x, currentNode.y + y, currentNode.z + z).pathfindingNode);
                    }
                }
            }
        }

        return new Vector3Int(-1, -1, -1);
    }

    public List<PathfindingNode> NodeWithinRange(Vector3Int start, int range)
    {
        resetGrid();

        gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode.rootDistance = 0;
        List<PathfindingNode> inRange = new List<PathfindingNode>();
        Queue<PathfindingNode> nodesToVisit = new Queue<PathfindingNode>();
        gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode.UpdateNode(0f, null);
        nodesToVisit.Enqueue(gridManager.grid.GetValue(start.x, start.y, start.z).pathfindingNode);

        while (nodesToVisit.Count > 0)
        {
            PathfindingNode node = nodesToVisit.Dequeue();
            Vector3Int currentCoord = node.coords;

            gridManager.grid.GetValue(currentCoord.x, currentCoord.y, currentCoord.z).pathfindingNode.visited = true;

            if(gridManager.grid.GetValue(currentCoord.x, currentCoord.y, currentCoord.z).pathfindingNode.rootDistance > range)
            {
                continue;
            }

            inRange.Add(node);



            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if (currentCoord.x + x > gridManager.grid.x - 1 || currentCoord.z + z > gridManager.grid.y - 1 || currentCoord.y + y > gridManager.grid.z - 1 || currentCoord.x + x < 0 || currentCoord.z + z < 0 || currentCoord.y + y < 0)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentCoord.x + x, currentCoord.y + y, currentCoord.z + z).pathfindingNode.visited)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentCoord.x + x, currentCoord.y + y, currentCoord.z + z).pathfindingNode.enterableSides[-x + 1, -y + 1, -z + 1] == false)
                        {
                            continue;
                        }
                        if (gridManager.grid.GetValue(currentCoord.x, currentCoord.y, currentCoord.z).pathfindingNode.enterableSides[x + 1, y + 1, z + 1] == false)
                        {
                            continue;
                        }

                        gridManager.grid.GetValue(currentCoord.x + x, currentCoord.y + y, currentCoord.z + z).pathfindingNode.ChangeRootDist(node.rootDistance+1);

                        nodesToVisit.Enqueue(gridManager.grid.GetValue(currentCoord.x + x, currentCoord.y + y, currentCoord.z + z).pathfindingNode);
                    }
                }
            }
        }
        return inRange;
    }

    void resetGrid()
    {
        for (int x = 0; x < gridManager.grid.x; x++)
        {
            for (int y = 0; y < gridManager.grid.y; y++)
            {
                for (int z = 0; z < gridManager.grid.z; z++)
                {
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.manhattenDistance = 0;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.rootDistance = Mathf.Infinity;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.distance = Mathf.Infinity;
                    gridManager.grid.GetValue(x, y, z).pathfindingNode.visited = false;
                }
            }
        }
    }
}
