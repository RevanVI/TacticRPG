using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsLastInTeam", menuName = "UtilityAI/IsLastInTeam", order = 101)]
public class IsLastInTeam : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        string fraction = context.Provider.GetControlledCharacter().tag;
        int counter = 0;
        foreach(var character in GameController.Instance.CharacterList)
        {
            if (character.tag == fraction)
                ++counter;
        }

        if (counter == 1)
            return 1f;
        else
            return 0f;
    }
}
