using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackOptionsPanel : MonoBehaviour
{
    [SerializeField] Transform contentBox;
    CombatUnit selectedUnit;
    [SerializeField] GameObject AttackCardPrefab;
    [SerializeField] CombatController controller;

    List<GameObject> attackCards = new List<GameObject>(); 

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupPanel(CombatUnit unit)
    {
        gameObject.SetActive(true);
        ClearPanel();
        selectedUnit = unit;

        foreach(AttackAbilitySO attack in unit.availableAttacks)
        {
            AttackCardScript card = Instantiate(AttackCardPrefab, contentBox).GetComponent<AttackCardScript>();
            card.Setup(attack, controller);
            attackCards.Add(card.gameObject);
        }
    }

    void ClearPanel()
    {
        foreach(GameObject attack in attackCards)
        {
            Destroy(attack);
        }
        attackCards.Clear();
    }
}
