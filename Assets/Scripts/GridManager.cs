using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Serializable3DArray<GridObject> grid;
    //public Serializable3DArray<SaveGridObject> saveGrid;
    
    public List<GameObject> buildingScripts;
    public Serializable1DArray<SaveGridObject> savedBuildings;

    public BuildingScript buildingScript;

    //public Dictionary<int, BuildingSectionSO> IDtoBlockMap = new Dictionary<int, BuildingSectionSO>();

    public List<BuildingObjectSO> IDtoBuildingMap;

    public void Reset(int setx, int sety, int setz)
    {

        grid = new Serializable3DArray<GridObject>(setx, sety, setz);
        for (int x = 0; x < grid.x; x++)
        {
            for (int z = 0; z < grid.z; z++)
            {
                for (int y = 0; y < grid.y; y++)
                {
                    //UtilsClass.CreateWorldText(x + ", " + y + ", " + z, null, GetWorldPosition(x, y, z) + new Vector3(tileSize, tileSize) * 0.5f, 5, Color.white, TextAnchor.MiddleCenter);
                    GridObject temp = new GridObject(x, y, z);
                    temp.pathfindingNode = new PathfindingNode(x, y, z);
                    grid.UpdateValue(x, y, z, temp);

                    //checkGrid[x, y, z] = Instantiate(gridNavPreviewObject, GetWorldPosition(new Vector3(x, y, z)), Quaternion.identity); 
                }
            }
        }
    }

    public void SaveToJson()
    {
        savedBuildings = new Serializable1DArray<SaveGridObject>(buildingScripts.Count);

        for(int i = 0; i< buildingScripts.Count; i++) { 
            savedBuildings.UpdateValue(i, buildingScripts[i].GetComponent<BuildingMasterScript>().GetSaveData()); 
        }

        string mapData = JsonUtility.ToJson(savedBuildings);
        string filePath = Application.persistentDataPath + "/MapData.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, mapData);
    }

    public void LoadFromJson()
    {
        buildingScripts.Clear();
        string filePath = Application.persistentDataPath + "/MapData.json";
        string mapData = System.IO.File.ReadAllText(filePath);
        Debug.Log(mapData);
        savedBuildings = JsonUtility.FromJson<Serializable1DArray<SaveGridObject>>(mapData);
        Debug.Log("loaded");

        for(int i = 0; i < savedBuildings.size; i++)
        {
            SaveGridObject obj = savedBuildings.GetValue(i);
            Vector3Int coord = new Vector3Int(obj.x, obj.y, obj.z);
            Vector3 coordWOffset = new Vector3(obj.x, obj.y, obj.z) + buildingScript.RotToOffset(obj.rot);
            buildingScript.AttemptBuild(coord, coordWOffset, true, IDtoBuildingMap[obj.buildingID]);
        }
    }



    public Dictionary<Vector3Int, GridObject> GetLineOfSight(Vector3Int start, int range)
    {
        Dictionary<Vector3Int, GridObject> inSight = new Dictionary<Vector3Int, GridObject>();
        for(int x = 0; x < range; x++)
        {
            for(int y = 0 ; y < range-x; y++)
            {
                for(int z=0 ; z < range-y-x ; z++)
                {
                    if (InLineOfSight(start, start + new Vector3Int(x, y, z))){
                        inSight[new Vector3Int(start.x + x, start.y + y, start.z + z)] = (grid.GetValue(start.x + x, start.y + y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(x, y, -z)))
                    {
                        inSight[new Vector3Int(start.x + x, start.y + y, start.z - z)] = (grid.GetValue(start.x + x, start.y + y, start.z - z));
                    }


                    if (InLineOfSight(start, start + new Vector3Int(x, -y, z)))
                    {
                        inSight[new Vector3Int(start.x + x, start.y - y, start.z + z)]  = (grid.GetValue(start.x + x, start.y - y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(x, -y, -z)))
                    {
                        inSight[new Vector3Int(start.x + x, start.y - y, start.z - z)] = (grid.GetValue(start.x + x, start.y - y, start.z - z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, y, z)))
                    {
                        inSight[new Vector3Int(start.x - x, start.y + y, start.z + z)] =(grid.GetValue(start.x - x, start.y + y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, y, -z)))
                    {
                        inSight[new Vector3Int(start.x - x, start.y + y, start.z - z)] = (grid.GetValue(start.x - x, start.y + y, start.z - z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, -y, z)))
                    {
                        inSight[new Vector3Int(start.x - x, start.y - y, start.z + z)] = (grid.GetValue(start.x - x, start.y - y, start.z + z));
                    }

                    if (InLineOfSight(start, start + new Vector3Int(-x, -y, -z)))
                    {
                        inSight[new Vector3Int(start.x - x, start.y - y, start.z - z)] = (grid.GetValue(start.x - x, start.y - y, start.z - z));
                    }
                }
            }
        }

        return inSight;
    }

    bool InLineOfSight(Vector3Int start, Vector3Int endLocation)
    {
        if (endLocation.x >= grid.x || endLocation.y >= grid.y || endLocation.z >= grid.y || endLocation.x < 0 || endLocation.y < 0 || endLocation.z < 0) { return false; }
        return !Physics.Linecast(buildingScript.GetWorldPositionCentre(start), buildingScript.GetWorldPositionCentre(endLocation));
    }
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
