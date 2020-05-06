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

        List<Node> path = GridSystem.Instance.BuildPath(currentCharacter.Coords, ((Character)context.Target).Coords, currentCharacter);
        List<KeyValuePair<int, Node.InfluenceStatus>> influenceData = GridSystem.Instance.GetInfluenceData(path[path.Count - 2].Coords);
        return 0f;
    }
}
