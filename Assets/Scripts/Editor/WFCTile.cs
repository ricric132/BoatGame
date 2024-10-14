using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCTile
{
    public int weight;

    public WFCTileSO SO;

    public bool possible;

    public WFCTile(WFCTileSO so)
    {
        this.weight = so.weight;
        possible = true;
        this.SO = so;
    }
}
