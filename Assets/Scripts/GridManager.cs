using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public Serializable3DArray<GridObject> grid;
    public List<GameObject> buildingScripts;

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

    public void SerializeToJson()
    {

    }
}
