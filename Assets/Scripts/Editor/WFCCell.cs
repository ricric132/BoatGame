using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WFCCell
{
    public List<WFCTile> allTiles;

    public int possible;

    public bool collapsed = false;

    public WFCTile selected;

    public WFCCell(List<WFCTile> allTiles)
    {
        this.allTiles = new List<WFCTile>();
        for (int i = 0; i < allTiles.Count; i++)
        {
            this.allTiles.Add(new WFCTile(allTiles[i].SO));
        }
        this.possible = allTiles.Count;
        this.collapsed = false;
    }

    public void UpdatePossible(Direction dir, WFCTileSO added)
    {
        if (collapsed)
        {
            return;
        }

        int p = 0;

        for (int i = 0; i < allTiles.Count; i++)
        {
            if (!allTiles[i].SO.isAllowed(dir, added))
            {
                allTiles[i].possible = false;
            }

            if (allTiles[i].SO.biases.ContainsKey(added))
            {
                allTiles[i].weight += allTiles[i].SO.biases[added];
            }

            if (allTiles[i].possible)
            {
                p++;
            }
        }

        possible = p;
    }

    public WFCTile CollapseCell()
    {
        int total = 0;
        for(int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i].possible)
            {
                total += allTiles[i].weight;
            }
        }

        int random = Random.Range(0, total+1);
        int saved = random;


        for (int i = 0; i < allTiles.Count; i++)
        {
            if (allTiles[i].possible)
            {
                random -= allTiles[i].weight;
                if (random <= 0)
                {
                    selected = allTiles[i];
                    break;
                }
            }
        }

        collapsed = true;

        if(selected == null)
        {
            return null;
        }
        Debug.Log(selected.SO.name);

       
        return selected;
    }

    public void ResetCell()
    {
        for (int i = 0; i < allTiles.Count; i++)
        {
            allTiles[i].possible = true;
            collapsed  = false;
        }
    }   
}

