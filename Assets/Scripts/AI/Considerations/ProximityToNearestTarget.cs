using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProximityToNearestTarget", menuName = "UtilityAI/ProximityToNearestTarget", order = 101)]
public class ProximityToNearestTarget : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        //define nearest target from current character's position
        float shortestDistance = float.MaxValue;
        Character nearestMeleeEnemy = null;
        //Character targetCharacter = null;
        string currentCharacterFraction = context.Provider.GetControlledCharacter().tag;
        foreach (var character in GameController.Instance.CharacterList)
        {
            if (character.Properties.CurrentHealth > 0 &&
                character.tag != currentCharacterFraction)
            { 
                float distance = (character.Coords - context.Provider.GetControlledCharacter().Coords).magnitude;
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestMeleeEnemy = character;
                }
            }
        }

        float newDistance = (nearestMeleeEnemy.Coords - (Vector3Int)context.Target).magnitude;

        //new << short -> score == 1
        //new < short -> 0 < score < 1
        //new == short -> score == 0
        //new >> short -> score < 0
        float score = 1 - newDistance / shortestDistance;

        return Mathf.Clamp01(score);
    }
}
