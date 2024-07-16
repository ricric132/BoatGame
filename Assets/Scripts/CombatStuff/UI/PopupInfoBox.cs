using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TMPro;
using UnityEngine;

public class PopupInfoBox : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] Transform healthBar;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(HoverPopupInfo info)
    {
        nameText.text = info.unitName;
        healthText.text = Mathf.Max(info.curHp, 0) + "/" + info.maxHp;
        healthBar.localScale = new Vector3(Mathf.Max((float)info.curHp) / (float)info.maxHp, 1, 1);
    }
}
