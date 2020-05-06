using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBalanceNearTile", menuName = "UtilityAI/CharacterBalanceNearTile", order = 101)]
public class CharacterBalanceNearTile : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        int allyCharacters = 0;
        int enemyCharacters = 0;

        Character currentCharacter = context.Provider.GetControlledCharacter();

        //calculate character balance on target tile
        List<KeyValuePair<int, Node.InfluenceStatus>> influenceData = GridSystem.Instance.GetInfluenceData((Vector3Int)context.Target);
        foreach (var pair in influenceData)
        {
            if (GameController.Instance.FindCharacter(pair.Key).tag == currentCharacter.tag)
                ++allyCharacters;
            else
                ++enemyCharacters;
        }

        float score = 1 + (allyCharacters - enemyCharacters) / 3;

        return Mathf.Clamp(score, 0, 1);
    }
}
