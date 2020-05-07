using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsTargetCharacterNotWarrior", menuName = "UtilityAI/IsTargetCharacterNotWarrior", order = 101)]
public class IsTargetCharacterNotWarrior : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        if (((Character)context.Target).Properties.Class != CharacterClass.Warrior)
            return 1f;
        return 0f;
    }
}
