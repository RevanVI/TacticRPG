using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetHP", menuName = "UtilityAI/TargetHP", order = 101)]
public class TargetHP : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character targetCharacter = (Character)context.Target;
        return ((float)targetCharacter.Properties.CurrentHealth) / targetCharacter.Properties.Health;
    }
}
