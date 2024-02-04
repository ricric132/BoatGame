using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Serializable3DArray<GridObject> grid;
    public List<GameObject> buildingScripts;
    public BuildingScript buildingScript;

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
        string mapData = JsonUtility.ToJson(grid);
        string filePath = Application.persistentDataPath + "/MapData.json";
        Debug.Log(filePath);
        System.IO.File.WriteAllText(filePath, mapData);
    }

    public void LoadFromJson()
    {
        string filePath = Application.persistentDataPath + "/MapData.json";
        string mapData = System.IO.File.ReadAllText(filePath);
        Debug.Log(mapData);
        grid = JsonUtility.FromJson<Serializable3DArray<GridObject>>(mapData);
        Debug.Log("loaded");

        for (int x = 0; x < grid.x; x++)
        {
            for(int z = 0;z < grid.z; z++)
            {
                for(int y = 0; y < grid.y;y++)
                {
                    if (grid.GetValue(x, y, z).section != null)
                    {
                        //still need to account for rotatiion
                        Instantiate(grid.GetValue(x, y, z).section.prefab, buildingScript.GetWorldPosition(x, y, z), Quaternion.identity);

                    }
                }
            }
        }
    }
}
