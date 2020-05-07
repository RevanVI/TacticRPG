using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterHPConsideration", menuName = "UtilityAI/CharacterHPConsideration", order = 101)]
public class CharacterHPConsideration : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character character = context.Provider.GetControlledCharacter();
        return ((float)character.Properties.CurrentHealth) / character.Properties.CurrentHealth;
    }
}
