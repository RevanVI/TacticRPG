using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsHealerInTeam", menuName = "UtilityAI/IsHealerInTeam", order = 101)]
public class IsHealerInTeam : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        string fraction = context.Provider.GetControlledCharacter().tag;

        foreach(var character in GameController.Instance.CharacterList)
        {
            if (character.Properties.CurrentHealth > 0 &&
                character.tag == fraction &&
                character.Properties.Class == CharacterClass.Healer)
                return 1f;
        }

        return 0f;
    }
}
