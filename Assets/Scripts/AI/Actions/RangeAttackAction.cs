using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RangeAttackAction", menuName = "UtilityAI/RangeAttackAction", order = 151)]
public class RangeAttackAction : ActionBase
{
    public override void Execute()
    {
        Character currentCharacter = Context.Provider.GetControlledCharacter();
        GridSystem.Instance.ResetMovemap();
        currentCharacter.AttackAtRange((Character)Context.Target);
    }
}
