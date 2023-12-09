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

    public class Node{
        public float distance;
        public float rootDistance;
        public float manhattenDistance;
        public bool[,,] enterableSides;
        public bool[,,] walls;
        public bool visited;
        public Node previousNode;
        public Vector3Int coords;

        public bool hasFloor;
        public Node(int x, int y, int z){
            hasFloor = false;
            enterableSides = new bool[3,3,3];
            walls = new bool[3,3,3];
            this.coords = new Vector3Int(x, y, z);
        }

        public void UpdateNode(float newDistance, Node prevNode){
            if(rootDistance < newDistance){
                return;
            }

            rootDistance = newDistance;
            distance = rootDistance + manhattenDistance;
            previousNode = prevNode;
        }

        public void UpdateNode(Node prevNode)
        {
            previousNode = prevNode;
        }

        public void ChangeRootDist(float newDist)
        {
            rootDistance = Mathf.Min(newDist, rootDistance);
        }
    }

    public Node[,,] Nodes;



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

    public List<Node> GetPath(Vector3Int start, Vector3Int destination){
        List<Node> path = new List<Node>();
        if(start == destination)
        {
            path.Add(Nodes[destination.x, destination.y, destination.z]);
            return path;
        }

        if(FindPath(start, destination) == false){
            Debug.Log("no Path found");
            return null;
        }

        Node prevStep = Nodes[destination.x, destination.y, destination.z];
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
        for (int x = 0; x < Nodes.GetLength(0); x++){
            for(int y = 0; y < Nodes.GetLength(1); y++){
                for(int z = 0; z < Nodes.GetLength(2); z++){
                    Nodes[x, y, z].manhattenDistance = Mathf.Abs(destination.x - x) + Mathf.Abs(destination.y - y) + Mathf.Abs(destination.z - z);
                    Nodes[x, y, z].rootDistance = Mathf.Infinity;
                    Nodes[x, y, z].distance = Mathf.Infinity;
                    Nodes[x, y, z].visited = false;
                }
            }
        }

        PriorityQueue<Node, float> nodesToVisit = new PriorityQueue<Node, float>(0);
        Nodes[start.x, start.y, start.z].UpdateNode(0f, null);
        nodesToVisit.Insert(Nodes[start.x, start.y, start.z], 0);
        
        while(nodesToVisit.Size() > 0){
            Vector3Int currentNode = nodesToVisit.Pop().coords;
            Nodes[currentNode.x, currentNode.y, currentNode.z].visited = true;



            for(int x = -1; x < 2; x++){
                for(int y = -1; y < 2; y++){
                    for(int z = -1; z < 2; z++){  
                        if(currentNode.x + x > Nodes.GetLength(0) -1 || currentNode.z + z > Nodes.GetLength(1) - 1|| currentNode.y + y > Nodes.GetLength(2) - 1|| currentNode.x + x < 0 || currentNode.z + z < 0 || currentNode.y + y < 0){
                            continue;
                        }
                        if(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].visited){
                            continue;
                        }
                        if(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].enterableSides[-x+1, -y+1, -z+1] == false){
                            continue;
                        }
                        if(Nodes[currentNode.x, currentNode.y, currentNode.z].enterableSides[x+1, y+1, z+1] == false){
                            continue;
                        }
                        
                        Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].UpdateNode(Nodes[currentNode.x, currentNode.y, currentNode.z].rootDistance + 1, Nodes[currentNode.x, currentNode.y, currentNode.z]);

                        if(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].coords == destination){
                            return true;
                        }
                        
                        nodesToVisit.Insert(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z], Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].distance);
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
            Nodes[start.x, start.y, start.z].UpdateNode(null);
            return start;
        }

        for (int x = 0; x < Nodes.GetLength(0); x++)
        {
            for (int y = 0; y < Nodes.GetLength(1); y++)
            {
                for (int z = 0; z < Nodes.GetLength(2); z++)
                {
                    Nodes[x, y, z].rootDistance = Mathf.Infinity;
                    Nodes[x, y, z].visited = false;
                }
            }
        }

        Queue<Node> nodesToVisit = new Queue<Node>();
        Nodes[start.x, start.y, start.z].UpdateNode(0f, null);
        nodesToVisit.Enqueue(Nodes[start.x, start.y, start.z]);

        while (nodesToVisit.Count() > 0)
        {
            Vector3Int currentNode = nodesToVisit.Dequeue().coords;
            Nodes[currentNode.x, currentNode.y, currentNode.z].visited = true;



            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        if (currentNode.x + x > Nodes.GetLength(0) - 1 || currentNode.z + z > Nodes.GetLength(1) - 1 || currentNode.y + y > Nodes.GetLength(2) - 1 || currentNode.x + x < 0 || currentNode.z + z < 0 || currentNode.y + y < 0)
                        {
                            continue;
                        }
                        if (Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].visited)
                        {
                            continue;
                        }
                        if (Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].enterableSides[-x + 1, -y + 1, -z + 1] == false)
                        {
                            continue;
                        }
                        if (Nodes[currentNode.x, currentNode.y, currentNode.z].enterableSides[x + 1, y + 1, z + 1] == false)
                        {
                            continue;
                        }

                        Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].UpdateNode(Nodes[currentNode.x, currentNode.y, currentNode.z].rootDistance + 1, Nodes[currentNode.x, currentNode.y, currentNode.z]);

                        if (destination.Contains(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].coords))
                        {
                            return Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z].coords;
                        }

                        nodesToVisit.Enqueue(Nodes[currentNode.x + x, currentNode.y + y, currentNode.z + z]);
                    }
                }
            }
        }

        return new Vector3Int(-1, -1, -1);
    }

    public List<Node> NodeWithinRange(Vector3Int start, int range)
    {
        resetGrid();

        Nodes[start.x, start.y, start.z].rootDistance = 0;
        List<Node> inRange = new List<Node>();
        Queue<Node> nodesToVisit = new Queue<Node>();
        Nodes[start.x, start.y, start.z].UpdateNode(0f, null);
        nodesToVisit.Enqueue(Nodes[start.x, start.y, start.z]);

        while (nodesToVisit.Count > 0)
        {
            Node node = nodesToVisit.Dequeue();
            Vector3Int currentCoord = node.coords;

            Nodes[currentCoord.x, currentCoord.y, currentCoord.z].visited = true;

            if(Nodes[currentCoord.x, currentCoord.y, currentCoord.z].rootDistance > range)
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
                        if (currentCoord.x + x > Nodes.GetLength(0) - 1 || currentCoord.z + z > Nodes.GetLength(1) - 1 || currentCoord.y + y > Nodes.GetLength(2) - 1 || currentCoord.x + x < 0 || currentCoord.z + z < 0 || currentCoord.y + y < 0)
                        {
                            continue;
                        }
                        if (Nodes[currentCoord.x + x, currentCoord.y + y, currentCoord.z + z].visited)
                        {
                            continue;
                        }
                        if (Nodes[currentCoord.x + x, currentCoord.y + y, currentCoord.z + z].enterableSides[-x + 1, -y + 1, -z + 1] == false)
                        {
                            continue;
                        }
                        if (Nodes[currentCoord.x, currentCoord.y, currentCoord.z].enterableSides[x + 1, y + 1, z + 1] == false)
                        {
                            continue;
                        }

                        Nodes[currentCoord.x + x, currentCoord.y + y, currentCoord.z + z].ChangeRootDist(node.rootDistance+1);

                        nodesToVisit.Enqueue(Nodes[currentCoord.x + x, currentCoord.y + y, currentCoord.z + z]);
                    }
                }
            }
        }
        return inRange;
    }

    void resetGrid()
    {
        for (int x = 0; x < Nodes.GetLength(0); x++)
        {
            for (int y = 0; y < Nodes.GetLength(1); y++)
            {
                for (int z = 0; z < Nodes.GetLength(2); z++)
                {
                    Nodes[x, y, z].manhattenDistance = 0;
                    Nodes[x, y, z].rootDistance = Mathf.Infinity;
                    Nodes[x, y, z].distance = Mathf.Infinity;
                    Nodes[x, y, z].visited = false;
                }
            }
        }
    }
}
