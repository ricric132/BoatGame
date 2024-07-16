using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "ScriptableObjects/Attack")]
public class AttackAbilitySO : ScriptableObject
{
    public string name;
    public CombatController.AttackType attackType;

    public int baseDamage;
    public CombatController.DamageType damageType;
    
    public int minRange;
    public int maxRange;

    public int weight;

    public float aoe;

  

}
