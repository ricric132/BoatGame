using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class OutlineIndicatorSides : MonoBehaviour
{
    [SerializeField] GameObject AB, AD, BC, DC, AE, BF, CG, DH, EF, EH, FG, HG;

    public void SetSides(Vector3Int pos, Dictionary<Vector3Int, GridObject> allPos)
    { 
        if(allPos.ContainsKey(pos + new Vector3Int(-1, -1, 0)))
        {
            AB.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(0, -1, -1)))
        {
            AD.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(0, -1, 1)))
        {
            BC.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(1, -1, 0)))
        {
            DC.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(-1, 0, -1)))
        {
            AE.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(-1, 0, 1)))
        {
            BF.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(1, 0, 1)))
        {
            CG.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(1, 0, -1)))
        {
            DH.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(-1, 1, 0)))
        {
            EF.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(0, 1, -1)))
        {
            EH.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(0, 1, 1)))
        {
            FG.SetActive(false);
        }

        if (allPos.ContainsKey(pos + new Vector3Int(1, 1, 0)))
        {
            HG.SetActive(false);
        }
    }
}
