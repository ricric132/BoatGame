using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitInfoPanel : MonoBehaviour
{
    public TextMeshProUGUI unitName;

    public void Setup(string name)
    {
        unitName.text = name;
    }
}
