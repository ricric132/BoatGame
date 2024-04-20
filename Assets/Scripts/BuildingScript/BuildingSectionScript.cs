using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSectionScript : MonoBehaviour, ITargetable
{
    public Vector3Int coords;
    int HP;
    int maxHP;
    public bool willGetHit;
    public Material baseMaterial;
    public Material hitIndicatorMaterial;
    public MeshRenderer visual;
    public List<Transform> aimableSpots;

    public DirectAimData aimData;

    public int pierceRequired;


    public DirectAimData GetLocation()
    {
        aimData.coords = coords;
        return aimData;
    }

    public void WillHit(bool hit)
    {
        if(hit)
        {
            visual.material = hitIndicatorMaterial;
        }
        else
        {
            visual.material = baseMaterial;
        }
    }
}
