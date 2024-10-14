using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationEvents : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] CombatUnit combat;

    public void EndCharacterAnimationStall()
    {
        combat.ResumePausedAction();
    }
}
