using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descText;
    public Quest displayedQuest;

    public void UpdateQuest(Quest quest)
    {
        displayedQuest = quest;
        titleText.text = quest.title;
        descText.text = quest.description;
    }
}
