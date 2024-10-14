using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGuy : MonoBehaviour
{
    public QuestUI questUI;
    public List<Quest> tasks = new List<Quest>();
    public Quest currentTask = null;

    // Start is called before the first frame update
    void Start()
    {
        currentTask = tasks[0];
        questUI = FindObjectOfType<QuestUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if(questUI.displayedQuest != currentTask)
        {
            questUI.UpdateQuest(currentTask);
        }
    }

    public void Complete(int index)
    {
        if(currentTask == tasks[index] && index < tasks.Count)
        {
            currentTask = tasks[index+1];
        }
    }
}
