using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack", menuName = "ScriptableObjects/Attack")]
public class AttackAbilitySO : ScriptableObject
{
    public CombatController.AttackType attackType;

    public int baseDamage;
    
    public int minRange;
    public int maxRange;
}
