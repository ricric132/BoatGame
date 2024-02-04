using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WorldPreset", menuName = "ScriptableObjects/WorldPreset")]
public class SavedWorldMapSO : ScriptableObject
{
    public Vector3Int gridDimensions;
    public List<SavedPlaceObject> savedPlaceObjects = new List<SavedPlaceObject>();
    public HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();

    public void Reset()
    {
        gridDimensions = Vector3Int.zero;
        savedPlaceObjects.Clear();
        occupied.Clear();
    }

    public void Save()
    {
        
    }

}

[Serializable]
public class SavedPlaceObject
{
    public BuildingObjectSO buildingObject;
    public BuildingScript.Rotation rot;
    public Vector3Int pos;

    public SavedPlaceObject(BuildingObjectSO buildingObject, BuildingScript.Rotation rot, Vector3Int pos)
    {
        this.buildingObject = buildingObject;
        this.rot = rot;
        this.pos = pos;
    }
}
