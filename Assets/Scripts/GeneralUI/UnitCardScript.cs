using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UnitCardScript : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;

    UnitManagerUI unitManagerUI;
    Transform canvas;
    Transform grid;

    Vector3 dragOffset;

    CharacterInfo info;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetupCard(CharacterInfo info, Transform grid, Transform canvas, UnitManagerUI unitManagerUI)
    {
        this.info = info;
        nameText.text = info.unitName;
        this.canvas = canvas;
        this.grid = grid;   
        this.unitManagerUI = unitManagerUI;
    }
    public void UpdateCard(CharacterInfo info)
    {
        nameText.text = info.unitName;
    }

    public void Click()
    {
        Debug.Log("click");
        dragOffset = Input.mousePosition - transform.position;
        transform.SetParent(canvas);
    }

    public void Drag()
    {
        transform.position = Input.mousePosition - dragOffset;
    }

    public void Release()
    {
        Transform newParent = unitManagerUI.CheckReleasePoint(info);

        StartCoroutine(MoveCard(newParent));
    }

    public IEnumerator MoveCard(Transform newParent)
    {
        transform.SetParent(newParent);

        transform.localPosition = new Vector3(0, 0, 0);

        yield return null;
    }
}
