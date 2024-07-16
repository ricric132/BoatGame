using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public interface ITargetable
{
    DirectAimData GetLocation();

    void WillHit(bool hit);
    void TakeDamage(int damage, CombatController.DamageType damageType, CombatUnit attacker, Vector3 damagePoint = default(Vector3));

    HoverPopupInfo GetHoverPopupInfo();

    GameObject GetGameObject();
    

}
