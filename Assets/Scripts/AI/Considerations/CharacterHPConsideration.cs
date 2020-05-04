using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterHConsideration", menuName = "UtilityAI/CharacterHPConsideration", order = 51)]
public class CharacterHPConsideration : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character character = context.Provider.GetControlledCharacter();
        return character.Properties.CurrentHealth / character.Properties.CurrentHealth;
    }
}
