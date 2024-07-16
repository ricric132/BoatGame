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

    public BuildingSectionSO buildingSectionSO;

    public List<MeshRenderer> visualSections;


    public DirectAimData GetLocation()
    {
        aimData.coords = coords;
        return aimData;
    }

    public void WillHit(bool hit)
    {
        foreach(MeshRenderer mesh in visualSections)
        {
            if(hit)
            {
                mesh.material = hitIndicatorMaterial;
            }
            else
            {
                mesh.material = baseMaterial;
            }
        }

    }

    public void TakeDamage(int damage, CombatController.DamageType damageType, CombatUnit attacker, Vector3 impactPoint = default(Vector3))
    {
        HP -= damage;
        if (HP <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void SetHP(int maxHP)
    {
        this.maxHP = maxHP;
        HP = maxHP;
    }

    public HoverPopupInfo GetHoverPopupInfo()
    {
        return new HoverPopupInfo(buildingSectionSO.displayName, maxHP, HP);
    }

    public GameObject GetGameObject() 
    { 
        return gameObject; 
    }
}
