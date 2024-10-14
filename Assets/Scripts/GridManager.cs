using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    List<GridHolder> gridHolders;
    public float tileSize = 1;
    public HashSet<Grid> grids = new HashSet<Grid>();

    public CompositeGrid combatGrid;

    //public Serializable3DArray<SaveGridObject> saveGrid;
    
    public List<GameObject> buildingScripts;
    public Serializable1DArray<SaveGridObject> savedBuildings;

    public BuildingScript buildingScript;

    //public Dictionary<int, BuildingSectionSO> IDtoBlockMap = new Dictionary<int, BuildingSectionSO>();

    public List<BuildingObjectSO> IDtoBuildingMap;

    public void Start()
    {
        gridHolders = new List<GridHolder>();
        foreach(GridHolder holder in FindObjectsOfType<GridHolder>())
        {
            gridHolders.Add(holder);
            grids.Add(holder.grid);
        }
        grids.Add(buildingScript.boatGrid);


    }

    public Grid AddGrid(int setx, int sety, int setz, Vector3 origin, float rotation)
    {
        Grid tempGrid = new Grid(setx, sety, setz, origin, rotation);
        grids.Add(tempGrid);
        
        return tempGrid;
    }

    public void SaveToJson()
    {
        savedBuildings = new Serializable1DArray<SaveGridObject>(buildingScripts.Count);

        for(int i = 0; i< buildingScripts.Count; i++) { 
            savedBuildings.UpdateValue(i, buildingScripts[i].GetComponent<BuildingMasterScript>().GetSaveData()); 
        }

        string mapData = JsonUtility.ToJson(savedBuildings);
        string filePath = Application.streamingAssetsPath + "/PlayerBoat.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, mapData);
    }

    public void LoadAllGrids()
    {
        LoadFromJson("PlayerBoat", buildingScript.boatGrid);
        foreach(GridHolder gridHolder in gridHolders)
        {
            gridHolder.PrepareForLoad();
            LoadFromJson(gridHolder.selectedPrefab.jsonName, gridHolder.grid);
        }
    }

    public void LoadPlayer()
    {
        LoadFromJson("PlayerBoat", buildingScript.boatGrid);
    }


    public void LoadFromJson(string jsonName, Grid targetGrid)
    {
        if(targetGrid == buildingScript.boatGrid)
        {
            buildingScripts.Clear();
        }
        //Debug.Log(targetGrid.gridRotation);
        
        string filePath = Application.streamingAssetsPath + "/" + jsonName + ".json";
        string mapData = System.IO.File.ReadAllText(filePath);
        //Debug.Log(mapData);
        savedBuildings = JsonUtility.FromJson<Serializable1DArray<SaveGridObject>>(mapData);
        //Debug.Log("loaded");

        for(int i = 0; i < savedBuildings.size; i++)
        {
            SaveGridObject obj = savedBuildings.GetValue(i);
            Vector3Int coord = new Vector3Int(obj.x, obj.y, obj.z);
            Vector3 coordWOffset = new Vector3(obj.x, obj.y, obj.z) + buildingScript.RotToOffset(obj.rot);
            buildingScript.AttemptBuild(coord, coordWOffset, true, IDtoBuildingMap[obj.buildingID], targetGrid, obj.rot);
        }
    }

    
    void transferFiles()
    {
        string to = Application.streamingAssetsPath + "/PlayerBoat.json";
        Debug.Log(to);
        string from = Application.persistentDataPath + "/PlayerBoat.json";
        string mapData = System.IO.File.ReadAllText(from);
        System.IO.File.WriteAllText(to, mapData);

        to = Application.streamingAssetsPath + "/EnemyBaseBoat.json";
        Debug.Log(to);
        from = Application.persistentDataPath + "/EnemyBaseBoat.json";
        mapData = System.IO.File.ReadAllText(from);
        System.IO.File.WriteAllText(to, mapData);
    }



    public Dictionary<Vector3Int, GridObject> GetLineOfSight(Vector3Int start, int range, Grid selectedGridObj)
    {
        Serializable3DArray<GridObject> grid = selectedGridObj.grid;
        Dictionary<Vector3Int, GridObject> inSight = new Dictionary<Vector3Int, GridObject>();
        for(int x = 0; x < range; x++)
        {
            for(int y = 0 ; y < range-x; y++)
            {
                for(int z=0 ; z < range-y-x ; z++)
                {
                    if (InLineOfSight(start, start + new Vector3Int(x, y, z), selectedGridObj)){
                        inSight[new Vector3Int(start.x + x, start.y + y, start.z + z)] = (grid.GetValue(start.x + x, start.y + y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(x, y, -z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x + x, start.y + y, start.z - z)] = (grid.GetValue(start.x + x, start.y + y, start.z - z));
                    }


                    if (InLineOfSight(start, start + new Vector3Int(x, -y, z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x + x, start.y - y, start.z + z)]  = (grid.GetValue(start.x + x, start.y - y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(x, -y, -z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x + x, start.y - y, start.z - z)] = (grid.GetValue(start.x + x, start.y - y, start.z - z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, y, z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x - x, start.y + y, start.z + z)] =(grid.GetValue(start.x - x, start.y + y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, y, -z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x - x, start.y + y, start.z - z)] = (grid.GetValue(start.x - x, start.y + y, start.z - z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, -y, z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x - x, start.y - y, start.z + z)] = (grid.GetValue(start.x - x, start.y - y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, -y, -z), selectedGridObj))
                    {
                        inSight[new Vector3Int(start.x - x, start.y - y, start.z - z)] = (grid.GetValue(start.x - x, start.y - y, start.z - z));
                    }
                }
            }
        }

        return inSight;
    }

    bool InLineOfSight(Vector3Int start, Vector3Int endLocation, Grid selectedGridObj)
    {
        Serializable3DArray<GridObject> grid = selectedGridObj.grid;
        if (endLocation.x >= grid.x || endLocation.y >= grid.y || endLocation.z >= grid.y || endLocation.x < 0 || endLocation.y < 0 || endLocation.z < 0) { return false; }
        return !Physics.Linecast(GetWorldPositionCentre(start, selectedGridObj), GetWorldPositionCentre(endLocation, selectedGridObj));
    }

    public Vector3 GetWorldPosition(int x, int y, int z, Grid selectedGridObj)
    {
        return new Vector3(x, y, z) * tileSize + selectedGridObj.origin;
    }

    public Vector3 GetWorldPosition(Vector3 coords, Grid selectedGridObj)
    {
        Vector3 pos = Quaternion.AngleAxis(selectedGridObj.gridRotation, Vector3.up) * coords;
        pos = pos * tileSize + selectedGridObj.origin;
        return pos;
    }

    public Vector3 GetWorldPositionCentre(Vector3 coords, Grid selectedGridObj)
    {
        Vector3 pos = Quaternion.AngleAxis(selectedGridObj.gridRotation, Vector3.up) * coords;
        pos = pos * tileSize + selectedGridObj.origin + Vector3.one * tileSize / 2;
        return pos;
    }


    public Vector3 GetRelativeWorldPosition(Vector3 coords, Grid selectedGridObj)
    {
        return new Vector3(coords.x, coords.y, coords.z) * tileSize + selectedGridObj.origin;
    }

    public Vector3Int GetXYZ(Vector3 worldPos, Grid selectedGridObj)
    {
        Vector3 recentered = worldPos - selectedGridObj.origin;
        recentered = Quaternion.AngleAxis(-selectedGridObj.gridRotation, Vector3.up) * recentered - (Vector3.one * tileSize / 2);
        return new Vector3Int(Mathf.RoundToInt(recentered.x / tileSize), Mathf.RoundToInt(recentered.y / tileSize), Mathf.RoundToInt(recentered.z / tileSize));
    }

    public Vector3Int GetRelativeXYZ(Vector3 Pos, Grid selectedGridObj)
    {
        Vector3 recentered = Pos;
        recentered = Quaternion.AngleAxis(-selectedGridObj.gridRotation, Vector3.up) * recentered;
        return new Vector3Int(Mathf.RoundToInt(recentered.x / tileSize), Mathf.Min(Mathf.RoundToInt(recentered.y / tileSize), 0), Mathf.RoundToInt(recentered.z / tileSize));
    }

    public void SetCombatGrid(List<Grid> grids)
    {
        combatGrid = new CompositeGrid(grids);
    }

    public List<Grid> FindGridsInRange(Vector2 centre, float range)
    {
        List<Grid> inRange = new List<Grid>();
        foreach(Grid grid in grids)
        {
            KeyValuePair<Vector2, float> circle = grid.GetCircleWrap();
            
            if(Vector2.Distance(circle.Key, centre) < range + circle.Value)
            {
                inRange.Add(grid);
            }
        }

        return inRange;
    }

    public void DecomposeGrid(CompositeGrid compGrid)
    {
        foreach (var component in compGrid.componentGrids)
        {
            Grid grid = component.Key;
            StitchData connection = component.Value;
            for (int x = 0; x < grid.grid.x; x++)
            {
                for (int z = 0; z < grid.grid.z; z++)
                {
                    for (int y = 0; y < grid.grid.y; y++)
                    {
                        int transX = (int)connection.startCoord.x + x;
                        int transY = (int)connection.startCoord.y + y;
                        int transZ = (int)connection.startCoord.z + z;

                        GridObject obj = compGrid.wholeGrid.grid.GetValue(transX, transY, transZ);

                        if(obj.owner == grid)
                        {
                            obj.RefreshCoords(x, y, z); 
                            grid.grid.UpdateValue(x, y, z, obj);
                        }
                        else
                        {
                            GridObject temp = new GridObject(x, y, z);
                            temp.pathfindingNode = new PathfindingNode(x, y, z);
                            temp.owner = grid;

                            grid.grid.UpdateValue(x, y, z, temp);
                        }
                    }
                }
            }
        }
        
    }

    public Grid GetPlayerBoat()
    {
        return buildingScript.boatGrid;
    }

    public List<Vector3> GetRandomWalkableCoords(int amount, Grid targetGrid)
    {
        List<Vector3> possible = new List<Vector3>();
        for (int x = 0; x < targetGrid.grid.x; x++)
        {
            for (int z = 0; z < targetGrid.grid.z; z++)
            {
                for (int y = 0; y < targetGrid.grid.y; y++)
                {
                    GridObject temp = targetGrid.grid.GetValue(x, y, z);
                    if (temp.pathfindingNode.hasFloor && !temp.occupied)
                    {
                        possible.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        List<Vector3> res = new List<Vector3>();

        for(int i = 0; i < amount; i++)
        {
            int random = UnityEngine.Random.Range(0, possible.Count);
            res.Add(possible[random]);
            possible.RemoveAt(random);
        }

        return res; 
    }

    /*
    public Vector3 FindMergeDirection(Dictionary<Grid, StitchData> grids, CompositeGrid composite) // x and z = dir       y = distance
    {
        foreach(var pair in grids)
        {
            Grid curGrid = pair.Key;
            if(curGrid != buildingScript.boatGrid)
            {
                float leftMost = curGrid.grid.x;
                float rightMost = 0;

                float frontMost = curGrid.grid.z;
                float backMost = 0;

                float bottomMost = curGrid.grid.y;
                float topMost = 0;

                for (int x = 0; x < curGrid.grid.x; x++)
                {
                    for (int z = 0; z < curGrid.grid.z; z++)
                    {
                        for (int y = 0; y < curGrid.grid.y; y++)
                        {
                            if (curGrid.grid.GetValue(x, y, z).occupied == true)
                            {
                                if (x < leftMost)
                                {
                                    leftMost = x;
                                }
                                if (x > rightMost)
                                {
                                    rightMost = x;
                                }

                                if (z < frontMost)
                                {
                                    frontMost = z;
                                }
                                if (z > backMost)
                                {
                                    backMost = z;
                                }

                                if(y < bottomMost)
                                {
                                    bottomMost = y;
                                }
                                if(y > topMost)
                                {
                                    topMost = y;
                                }
                            }
                        }
                    }
                }

                for(int x = (int)leftMost; x < rightMost; x++)
                {
                   for(int y = (int)bottomMost; y < topMost; y++)
                   {
                        for(int z = (int)backMost; z < composite.wholeGrid.grid.z; z++)
                        {
                            if(composite.wholeGrid.grid.GetValue(x, y, z).occupied && composite.wholeGrid.grid.GetValue(x, y, z).owner == buildingScript.boatGrid)
                            {
                                return new Vector3(0, z - frontMost, 1);
                            }
                        }

                        for (int z = (int)frontMost; z >= 0; z--)
                        {
                            if (composite.wholeGrid.grid.GetValue(x, y, z).occupied && composite.wholeGrid.grid.GetValue(x, y, z).owner == buildingScript.boatGrid)
                            {
                                return new Vector3(0, backMost - z, -1);
                            }
                        }
                   }
                }

                for (int z = (int)frontMost; z < backMost; z++)
                {
                    for (int y = (int)bottomMost; y < topMost; y++)
                    {
                        for (int x = (int)rightMost; z < composite.wholeGrid.grid.x; z++)
                        {
                            if (composite.wholeGrid.grid.GetValue(x, y, z).occupied && composite.wholeGrid.grid.GetValue(x, y, z).owner == buildingScript.boatGrid)
                            {
                                return new Vector3(1, x-rightMost, 0);
                            }
                        }

                        for (int x = (int)leftMost; z >= 0; z--)
                        {
                            if (composite.wholeGrid.grid.GetValue(x, y, z).occupied && composite.wholeGrid.grid.GetValue(x, y, z).owner == buildingScript.boatGrid)
                            {
                                return new Vector3(-1,  , 0);
                            }
                        }
                    }
                }
            }
        }
    }*/
}

[Serializable]
public class ToSave
{
    List<SaveGridObject> save;
    public ToSave(List<SaveGridObject> list)
    {
        this.save = list;
    }
}

public class Grid
{
    public Serializable3DArray<GridObject> grid;
    public Vector3 origin = new Vector3(0, 0, 0);
    public float gridRotation;
    public Transform parentTransform;

    public Grid()
    {
        grid = new Serializable3DArray<GridObject>(1, 1, 1);
    }

    public Grid(Serializable3DArray<GridObject> _grid, Vector3 _origin, float _gridRotation)
    {
        grid = _grid;
        origin = _origin;
        gridRotation = _gridRotation;
    }

    public Grid(int setx, int sety, int setz, Vector3 origin, float rotation)
    {
        grid = new Serializable3DArray<GridObject>(setx, sety, setz);
        this.origin = origin;
        this.gridRotation = rotation;

        for (int x = 0; x < grid.x; x++)
        {
            for (int z = 0; z < grid.z; z++)
            {
                for (int y = 0; y < grid.y; y++)
                {
                    //UtilsClass.CreateWorldText(x + ", " + y + ", " + z, null, GetWorldPosition(x, y, z) + new Vector3(tileSize, tileSize) * 0.5f, 5, Color.white, TextAnchor.MiddleCenter);
                    GridObject temp = new GridObject(x, y, z);
                    temp.pathfindingNode = new PathfindingNode(x, y, z);
                    temp.owner = this;

                    grid.UpdateValue(x, y, z, temp);

                    //checkGrid[x, y, z] = Instantiate(gridNavPreviewObject, GetWorldPosition(new Vector3(x, y, z)), Quaternion.identity); 
                }
            }
        }
    }

    public KeyValuePair<Vector2, float> GetCircleWrap()
    {
        float leftMost = grid.x;
        float rightMost = 0;
        float frontMost = grid.z;
        float backMost = 0;
        for (int x = 0; x < grid.x; x++)
        {
            for (int z = 0; z < grid.z; z++)
            {
                for (int y = 0; y < grid.y; y++)
                {
                    if (grid.GetValue(x, y, z).occupied == true)
                    {
                        if(x < leftMost)
                        {
                            leftMost = x;
                        }
                        if(x > rightMost)
                        {
                            rightMost = x;
                        }
                        if(z < frontMost)
                        {
                            frontMost = z;
                        }
                        if(z > backMost)
                        {
                            backMost = z;
                        }
                    }
                }
            }
        }


        Vector3 centreV3 = Quaternion.AngleAxis(gridRotation, Vector3.up) *  new Vector3((leftMost+rightMost)/2, 0, (frontMost + backMost)/2) + origin;
        Vector2 centre = new Vector2(centreV3.x, centreV3.z);
        float radius = Mathf.Max((rightMost - leftMost), (frontMost - backMost));

        return new KeyValuePair<Vector2, float>(centre, radius);


    }

}

public class CompositeGrid
{
    public Dictionary<Grid, StitchData> componentGrids = new Dictionary<Grid, StitchData>(); //Key = Grid, Value = amount ouf clockwise rotations

    public Grid wholeGrid;

    public void SetStartCoord(Grid grid, StitchData stitchData, Vector3 origin)
    {
        Vector3 offset = stitchData.standardizedOrigin - origin;
        stitchData.startCoord = new Vector3(Mathf.Round(offset.x), Mathf.Round(offset.y), Mathf.Round(offset.z));

        Debug.Log(stitchData.startCoord - offset);
        grid.parentTransform.position += stitchData.startCoord - offset;

    }

    public Vector3 RotateCoord(Vector3 coord, int rotAC, int Xmax, int Zmax)
    {
        int x = (int)coord.x;
        int y = (int)coord.y;
        int z = (int)coord.z;

        //works for square only need to fix Xmax snd Ymax for rectagles 
        for (int i = 0; i < rotAC; i++)
        {
            int temp = x;
            x = z;
            z = Xmax - temp;
        }

        return new Vector3Int(x, y, z);
    }

    public CompositeGrid(List<Grid> grids)
    {
        componentGrids = new Dictionary<Grid, StitchData>();
        float leftMost = grids[0].origin.x;
        float botMost = grids[0].origin.y;
        float frontMost = grids[0].origin.z;

        float rightMost = grids[0].origin.x;
        float topMost = grids[0].origin.y;
        float backMost = grids[0].origin.z;

        foreach (Grid grid in grids)
        {
            float newRot = Mathf.Round(grid.gridRotation / 90) * 90;
            float deltaRot = newRot - grid.gridRotation;

            grid.parentTransform.Rotate(new Vector3(0, deltaRot, 0));

            Debug.Log(grid);

            int RotC = Mathf.RoundToInt(newRot / 90);

            Vector3 standardizedOrigin;
            Vector3 endPoint;

            if(RotC == 0)
            {
                standardizedOrigin = grid.origin;
                endPoint = grid.origin + new Vector3(grid.grid.x, grid.grid.y, grid.grid.z);
            }
            else if(RotC == 1)
            {
                standardizedOrigin = grid.origin - new Vector3(0, 0, grid.grid.x);
                endPoint = grid.origin + new Vector3(grid.grid.z, grid.grid.y, grid.grid.x);
            }
            else if(RotC == 2)
            {
                endPoint = grid.origin + new Vector3(0, grid.grid.y, 0);
                standardizedOrigin = grid.origin - new Vector3(grid.grid.x, 0, grid.grid.z);
            }
            else
            {
                endPoint = grid.origin + new Vector3(0, grid.grid.y, grid.grid.x);
                standardizedOrigin = grid.origin - new Vector3(grid.grid.z, 0, 0);
            }

            StitchData stitchData = new StitchData(RotC, standardizedOrigin);

            componentGrids.Add(grid, stitchData);

            if(standardizedOrigin.x < leftMost)
            {
                leftMost = standardizedOrigin.x;
            }

            if(standardizedOrigin.y < botMost)
            {
                botMost = standardizedOrigin.y;
            }

            if(standardizedOrigin.z < frontMost)
            {
                frontMost = standardizedOrigin.z;
            }

            if (endPoint.x > rightMost)
            {
                rightMost = endPoint.x;
            }

            if (endPoint.y > topMost)
            {
                topMost = endPoint.y;
            }

            if (endPoint.z > backMost)
            {
                backMost = endPoint.z;
            }
        }

        //Debug.Log((rightMost - leftMost) + ", " + (topMost - botMost) + ", " + (backMost - frontMost));

        wholeGrid = new Grid(Mathf.CeilToInt(rightMost - leftMost), Mathf.CeilToInt(topMost - botMost), Mathf.CeilToInt(backMost - frontMost), new Vector3(leftMost, botMost, frontMost), 0);

        Debug.Log(wholeGrid.grid.x + ", " + wholeGrid.grid.y + ", " + wholeGrid.grid.z);
        
        foreach(var component in componentGrids)
        {
            SetStartCoord(component.Key, component.Value, wholeGrid.origin);

            for (int x = 0; x < component.Key.grid.x; x++)
            {
                for (int z = 0; z < component.Key.grid.z; z++)
                {
                    for (int y = 0; y < component.Key.grid.y; y++)
                    {
                        Vector3 transformedCoord = component.Value.startCoord + RotateCoord(new Vector3(x, y, z), component.Value.clockwiseRotations, component.Key.grid.x, component.Key.grid.z);
                        if (wholeGrid.grid.GetValue((int)transformedCoord.x, (int)transformedCoord.y, (int)transformedCoord.z).pathfindingNode.Equals(new PathfindingNode((int)transformedCoord.x, (int)transformedCoord.y, (int)transformedCoord.z)))
                        {
                            //Debug.Log("empty: " + transformedCoord);
                            wholeGrid.grid.UpdateValue((int)transformedCoord.x, (int)transformedCoord.y, (int)transformedCoord.z, component.Key.grid.GetValue(x, y, z));
                            wholeGrid.grid.GetValue((int)transformedCoord.x, (int)transformedCoord.y, (int)transformedCoord.z).RefreshCoords((int)transformedCoord.x, (int)transformedCoord.y, (int)transformedCoord.z);
                        }
                        else
                        {
                            
                        }
                    }
                }
            }
        }
    }
}

public class StitchData
{
    public int clockwiseRotations;
    public Vector3 standardizedOrigin;
    public Vector3 startCoord;
    
    public StitchData(int rot, Vector3 origin)
    {
        clockwiseRotations = rot;
        standardizedOrigin = origin;
    }

}
