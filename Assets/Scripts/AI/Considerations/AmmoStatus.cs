using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AmmoStatus", menuName = "UtilityAI/AmmoStatus", order = 101)]
public class AmmoStatus : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character currentCharacter = context.Provider.GetControlledCharacter();
        if (currentCharacter.Properties.MaxMissiles == 0)
            return 0f;
        return ((float)currentCharacter.Properties.CurrentMissiles) / currentCharacter.Properties.MaxMissiles;
    }
}
