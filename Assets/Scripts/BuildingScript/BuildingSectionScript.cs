using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingSectionScript : MonoBehaviour, ITargetable
{
    public Vector3Int coords;
    int HP;
    int maxHP;
    public bool willGetHit;
    public Material hitIndicatorMaterial;
    public List<Transform> aimableSpots;

    public DirectAimData aimData;

    public int pierceRequired;

    public BuildingSectionSO buildingSectionSO;

    public List<MeshRenderer> visualSections;
    Dictionary<MeshRenderer, Material> baseMaterials;


    void Start()
    {
    }

    public DirectAimData GetLocation()
    {
        aimData.coords = coords;
        return aimData;
    }

    public void WillHit(bool hit)
    {
        foreach(MeshRenderer mesh in visualSections)
        {
            baseMaterials[mesh] = mesh.material;
            if (hit)
            {
                mesh.material = hitIndicatorMaterial;
            }
            else
            {
                mesh.material = baseMaterials[mesh];
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
