using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsTargetStunned", menuName = "UtilityAI/IsTargetStunned", order = 101)]
public class IsTargetStunned : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        float score = 1f;

        foreach (var effect in ((Character)context.Target).ActiveEffects)
            if (effect.Type == EffectType.Stun)
                return 1f;

        return 0f;
    }
}
