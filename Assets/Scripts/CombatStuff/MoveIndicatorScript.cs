using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MoveIndicatorScript : MonoBehaviour
{
    public Vector3Int Coords;
    public CombatController combatController;
    public bool hovered;

    void Update()
    {
        if (hovered)
        {
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        else
        {
            transform.localScale = Vector3.one;
        }
        hovered = false;
    }

}
