using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AttackCardScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI damageText;
    [SerializeField] TextMeshProUGUI rangeText;

    [SerializeField] Button button;

    CombatController combatController;
    AttackAbilitySO selectedAttack;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(AttackAbilitySO attack, CombatController controller)
    {
        combatController = controller;
        nameText.text = attack.name;
        damageText.text = attack.baseDamage.ToString();
        selectedAttack = attack;

        if (attack.attackType == CombatController.AttackType.Direct)
        {   
            rangeText.text = attack.minRange + "-" + attack.maxRange;
        }
        else if(attack.attackType == CombatController.AttackType.Lob)
        {
            rangeText.text = attack.weight + "kg";
        }

        button.onClick.AddListener(ButtonOnClick);
    }

    void ButtonOnClick()
    {
        combatController.SelectAttack(selectedAttack);
    }
}
