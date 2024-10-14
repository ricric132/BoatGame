using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CantBuildPopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;

    

    public void Popup(string str)
    {
        gameObject.SetActive(true); 
        text.text = str;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
