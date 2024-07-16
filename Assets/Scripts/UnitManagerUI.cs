using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class UnitManagerUI : MonoBehaviour
{
    [SerializeField] PlayerUnitsManager playerUnitsManager;
    [SerializeField] Transform unitGrid;
    [SerializeField] Transform canvas;
    [SerializeField] GameObject unitCardPrefab;
    Dictionary<CharacterInfo, GameObject> cards = new Dictionary<CharacterInfo, GameObject>();

    [SerializeField] List<GameObject> selectionSlots = new List<GameObject>();
    

    int hovered; 
    
   

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateUnitGrid();
    }

    void UpdateUnitGrid()
    {
        foreach(CharacterInfo c in playerUnitsManager.allUnits) {
            if(cards.ContainsKey(c))
            {
                UpdateCard(c, cards[c]);
            }
            else
            {
                AddCard(c);
            }
        }
    }

    void UpdateCard(CharacterInfo info, GameObject card)
    {
        card.GetComponent<UnitCardScript>().UpdateCard(info);
    }

    void AddCard(CharacterInfo info)
    {
        GameObject GO = Instantiate(unitCardPrefab, unitGrid);
        GO.GetComponent<UnitCardScript>().SetupCard(info, unitGrid, canvas, this);
        cards.Add(info, GO);
    }

    public Transform CheckReleasePoint(CharacterInfo info)
    {
        int prevIndex = playerUnitsManager.RemoveUnit(info);
        int ind = 0;
        foreach(GameObject box in selectionSlots)
        {
            float w = box.GetComponent<RectTransform>().rect.width;
            float h = box.GetComponent<RectTransform>().rect.height;
            Vector3 dist = Input.mousePosition - box.transform.position;

            if (Mathf.Abs(dist.x) <= w/2 && Mathf.Abs(dist.y) <= h/2)
            {
             
                CharacterInfo prev = playerUnitsManager.SelectTeamUnit(ind, info, prevIndex);
                if(prev != null)
                {
                    if(prevIndex == -1)
                    {
                        StartCoroutine(cards[prev].GetComponent<UnitCardScript>().MoveCard(unitGrid));
                    }
                    else
                    {
                        StartCoroutine(cards[prev].GetComponent<UnitCardScript>().MoveCard(selectionSlots[prevIndex].transform));
                    }
                }
                return box.transform;
            }
            ind++;
        }

        return unitGrid;
    }

    public void EnterDropArea(int ind)
    {
        hovered = ind;
    }
    public void ExitDropArea()
    {
        hovered = -1;
    }




}


