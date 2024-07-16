using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ControllableUnitSideTab : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI hpText;
    [SerializeField] Transform hpBar;

    string savedName;
    float curHP;
    float maxHP;

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
        maxHP = info.maxHp;
        curHP = Mathf.Max(info.curHp, 0);
        savedName = info.unitName;
        nameText.text = savedName;
        hpText.text = curHP + "/" + maxHP;
        hpBar.localScale = new Vector3(curHP / maxHP, 1, 1);

    }

    public IEnumerator updateHP(float newHP)
    {
        float totalTime = 0.5f;
        float curTime = 0;
        while(curTime < totalTime)
        {
            curTime += Time.deltaTime;
            hpBar.localScale = new Vector3(Mathf.Lerp(curHP, newHP, curTime/totalTime)/maxHP, 1, 1);
            hpText.text = Mathf.FloorToInt(Mathf.Lerp(curHP, newHP, curTime / totalTime)) + "/" + maxHP;
            yield return null;
        }
        curHP = newHP;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
