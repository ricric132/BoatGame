using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatUIManager : MonoBehaviour
{
    [SerializeField] GameObject ActionSelect;
    [SerializeField] GameObject AttackSelect;
    [SerializeField] GameObject HoverPopup;
    [SerializeField] List<ControllableUnitSideTab> unitSideTab = new List<ControllableUnitSideTab>();

    public enum CombatUIPhase
    {
        None,
        ActionSelect,
        AttackSelect,
        OutOfCombat
    }

    CombatUIPhase currentUIPhase;

    public void SetUI(CombatUIPhase phase, CombatUnit unit = null)
    {
        if(phase == CombatUIPhase.None)
        {
            gameObject.SetActive(true);
            ActionSelect.SetActive(false);
            AttackSelect.SetActive(false);
        }

        if(phase == CombatUIPhase.ActionSelect) { 
            gameObject.SetActive(true);
            ActionSelect.SetActive(true);
            AttackSelect.SetActive(false);
            if(unit != null)
            {
                ActionSelect.GetComponent<UnitInfoPanel>().Setup(unit);
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

    public void ShowHoverPopupInfo(HoverPopupInfo info)
    {
        HoverPopup.SetActive(true);
        HoverPopup.GetComponent<PopupInfoBox>().Setup(info);
    }

    public void HideHoverPopup()
    {
        HoverPopup.SetActive(false);
    }

    public void SetupSideBar(List<CombatUnit> controllable)
    {
        for (int i = 0; i < 4; i++)
        {
            if (controllable[i] != null)
            {
                unitSideTab[i].Setup(controllable[i].GetHoverPopupInfo());
            }
            else
            {
                unitSideTab[i].Hide();
            }
        }
    }

    public void UpdateSidebarHP(int index, float newHP)
    {
        StartCoroutine(unitSideTab[index].updateHP(newHP));
    }
    
}

public class HoverPopupInfo
{
    public string unitName;
    public int maxHp;
    public int curHp;
    //add statuses and stats

    public HoverPopupInfo(string _unitName, int _maxHP, int _curHp)
    {
        unitName = _unitName;
        maxHp = _maxHP; 
        curHp = _curHp;
    }
}
