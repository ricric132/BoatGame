using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI unitName;
    public TextMeshProUGUI HP;
    public TextMeshProUGUI Movement;
    public TextMeshProUGUI Actions;

    public void Setup(CombatUnit unit)
    {
        unitName.text = unit.GetName();
        HP.text = "HP - " + unit.curHP + " / " + unit.maxHP;
        Movement.text = "Remaining Movement - " + unit.remainingMovement + " / " + unit.stats.SPD;
        Actions.text = "Remaining Actions - " + unit.actionsLeft + " / " + unit.maxActions;

    }
}
