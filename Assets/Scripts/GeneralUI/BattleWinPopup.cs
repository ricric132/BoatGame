using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleWinPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI mainText;

    public void PopUp(int amount)
    {
        gameObject.SetActive(true);
        mainText.text = "You win the battle. Recources gained from savaging enemy ship: " + amount + "Wood"; 
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
