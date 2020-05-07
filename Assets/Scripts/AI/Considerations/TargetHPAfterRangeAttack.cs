using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetHPAfterRangeAttack", menuName = "UtilityAI/TargetHPAfterRangeAttack", order = 101)]
public class TargetHPAfterRangeAttack : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character targetCharacter = (Character)context.Target;
        int currentHP = targetCharacter.Properties.CurrentHealth - context.Provider.GetControlledCharacter().Properties.RangedDamage;
        if (currentHP < 0)
            currentHP = 0;
        return ((float)currentHP) / targetCharacter.Properties.Health;
    }
}
