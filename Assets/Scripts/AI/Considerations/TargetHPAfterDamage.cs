using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetHPAfterDamage", menuName = "UtilityAI/TargetHPAfterDamage", order = 101)]
public class TargetHPAfterDamage : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character targetCharacter = (Character)context.Target;
        int currentHP = targetCharacter.Properties.CurrentHealth - context.Provider.GetControlledCharacter().Properties.MeleeDamage;
        if (currentHP < 0)
            currentHP = 0;
        return currentHP / targetCharacter.Properties.Health;
    }
}
