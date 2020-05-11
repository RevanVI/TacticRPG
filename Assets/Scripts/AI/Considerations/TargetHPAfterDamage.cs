using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetHPAfterDamage", menuName = "UtilityAI/TargetHPAfterDamage", order = 101)]
public class TargetHPAfterDamage : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character targetCharacter = (Character)context.Target;
        Character currentCharacter = context.Provider.GetControlledCharacter();
        int currentHP = targetCharacter.Properties.CurrentHealth - currentCharacter.CalculateDamage(currentCharacter.Properties.MeleeDamage);
        if (currentHP < 0)
            currentHP = 0;
        return ((float)currentHP) / targetCharacter.Properties.Health;
    }
}
