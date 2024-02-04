using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Building", menuName = "ScriptableObjects/Building")]
public class BuildingObjectSO : ScriptableObject
{
    public GameObject prefab;
    public GameObject preview;
    public int x;
    public int y;
    public int z;
    public Serializable3DArray<BuildingSectionSO> sections;
    public SerializableDictionary<ResourceSO, int> buildingResources;
    public bool topWalkable;
    public EntryWay[] entryWays;
    public int[] walkableLevels;
    public string buildingName;
    public string buildingDescription;
    public Image icon;
    public int ID;
    public GameObject scriptInstancePrefab;
}

[Serializable]
public class EntryWay{
    public Vector3Int position;
    public Vector3Int[] directions;
    
}
