using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CanRunAway", menuName = "UtilityAI/CanRunAway", order = 101)]
public class CanRunAway : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character character = context.Provider.GetControlledCharacter();
        List<KeyValuePair<int, Node.InfluenceStatus>> influenceData = GridSystem.Instance.GetInfluenceData(character.Coords);

        float score = 0f;
        int enemyCounter = 0;
        foreach (var data in influenceData)
        {
            Character targetCharacter = GameController.Instance.FindCharacter(data.Key);
            if (targetCharacter.tag != character.tag &&
                data.Value == Node.InfluenceStatus.MeleeAttack)
            {
                float value = targetCharacter.Properties.Speed / character.Properties.Speed;
                //if character can't run away from target consider it
                if (value >= 1)
                    score += 1;
                ++enemyCounter;
            }
        }

        /*
         * 0 - can run away from all or no enemies near
         * 0-1 - can run away from some amount of enemies
         * 1 - can't run away from any enemy
         */ 
        if (enemyCounter == 0)
            return 0f;
        else
        {
            return score / enemyCounter;
        }
    }
}
