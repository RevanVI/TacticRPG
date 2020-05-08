using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsDangerOfMeleeAttack", menuName = "UtilityAI/IsDangerOfMeleeAttack", order = 101)]
public class IsDangerOfMeleeAttack : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character character = context.Provider.GetControlledCharacter();
        List<KeyValuePair<int, Node.InfluenceStatus>> influenceData = GridSystem.Instance.GetInfluenceData(character.Coords);

        foreach(var data in influenceData)
        {
            Character targetCharacter = GameController.Instance.FindCharacter(data.Key);
            if (targetCharacter.tag != character.tag)
                return 1f;
        }

        return 0f;
    }
}
