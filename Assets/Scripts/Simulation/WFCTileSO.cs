using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "WFCTileSO", menuName = "ScriptableObjects/WFCTileSO")]
public class WFCTileSO : ScriptableObject
{
    public int weight;

    [SerializeField] List<WFCTileSO> upAllowed;
    [SerializeField] List<WFCTileSO> downAllowed;
    [SerializeField] List<WFCTileSO> leftAllowed;
    [SerializeField] List<WFCTileSO> rightAllowed;
    [SerializeField] List<WFCTileSO> frontAllowed;
    [SerializeField] List<WFCTileSO> backAllowed;

    public SerializableDictionary<WFCTileSO, int> biases;

    public BuildingObjectSO buildingObject;

    public bool isAllowed(Direction dir, WFCTileSO tile)
    {
        if(dir == Direction.up)     {return upAllowed.Contains(tile);}
        if(dir == Direction.down)   {return downAllowed.Contains(tile);}
        if(dir == Direction.left)   {return leftAllowed.Contains(tile);}
        if(dir == Direction.right)  {return rightAllowed.Contains(tile);}
        if(dir == Direction.front)  {return frontAllowed.Contains(tile);}
        if(dir == Direction.back)   {return backAllowed.Contains(tile);}

        return false;
    }
}