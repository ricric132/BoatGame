using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUIManager : MonoBehaviour
{
    [SerializeField] GameObject ActionSelect;
    [SerializeField] GameObject AttackSelect;

    public enum CombatUIPhase
    {
        None,
        ActionSelect,
        AttackSelect
    }

    CombatUIPhase currentUIPhase;

    public void SetUI(CombatUIPhase phase, CombatUnit unit = null)
    {
        if(phase == CombatUIPhase.None)
        {
            gameObject.SetActive(false);
            ActionSelect.SetActive(false);
            AttackSelect.SetActive(false);
        }

        if(phase == CombatUIPhase.ActionSelect) { 
            gameObject.SetActive(true);
            ActionSelect.SetActive(true);
            AttackSelect.SetActive(false);
            if(unit != null && currentUIPhase != phase)
            {
                //setup action pannel
            }
        }

        if (phase == CombatUIPhase.AttackSelect)
        {
            gameObject.SetActive(true);
            ActionSelect.SetActive(false);
            AttackSelect.SetActive(true);
            if (unit != null && currentUIPhase != phase)
            {
                AttackSelect.GetComponent<AttackOptionsPanel>().SetupPanel(unit);
            }
        }

        currentUIPhase = phase;
    }

    public CombatUIPhase GetPhase()
    {
        return currentUIPhase;
    }

    public void ReturnToActionSelect()
    {
        SetUI(CombatUIPhase.ActionSelect);
    }
}
