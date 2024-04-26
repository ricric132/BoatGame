using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatAction 
{
    public AttackAbilitySO action;
    public DirectAimData target;

    public ActionType type;
    public List<PathfindingNode> movementPath;

    public CombatAction(AttackAbilitySO _action, DirectAimData _target) {
        this.action = _action;
        this.target = _target;
        type = ActionType.Attack;
    }

    public CombatAction(List<PathfindingNode> path)
    {
        type = ActionType.Move; 
        movementPath = path;
    }
}

public enum ActionType{
    Attack, 
    Move
}
