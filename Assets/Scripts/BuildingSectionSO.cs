using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "ScriptableObjects/BuildingSection")]
public class BuildingSectionSO : ScriptableObject
{
    public GameObject prefab;
    public GameObject masterScripts;
    public Vector3Int[] walkableDirs;
    public int x;
    public int y;
    public int z;

}

