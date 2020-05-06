using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterBalanceNearTargetCharacter", menuName = "UtilityAI/CharacterBalanceNearTargetCharacter", order = 101)]
public class CharacterBalanceNearTargetCharacter : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        int allyCharacters = 0;
        int enemyCharacters = 0;

        Character currentCharacter = context.Provider.GetControlledCharacter();

        //get tile from that character will be attack
        Path path = GridSystem.Instance.BuildPath(currentCharacter.Coords, ((Character)context.Target).Coords, currentCharacter);

        //calculate character balance on found tile
        List<KeyValuePair<int, Node.InfluenceStatus>> influenceData = GridSystem.Instance.GetInfluenceData(path.NodePath[path.NodePath.Count - 2].Coords);
        foreach(var pair in influenceData)
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
