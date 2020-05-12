using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillAction", menuName = "UtilityAI/SkillAction", order = 151)]
public class SkillAction : ActionBase
{
    //get skill
    public override void Execute()
    {
        Character currentCharacter = Context.Provider.GetControlledCharacter();
        int skillNo = (int)Context.Data["SkillNo"];

        //build path if needed
        object buf;
        bool needToMove = Context.Data.TryGetValue("AttackTile", out buf);
        List<Vector3Int> coordsPath = new List<Vector3Int>();
        if (needToMove)
        {
            coordsPath = GridSystem.Instance.BuildPath(currentCharacter.Coords, (Vector3Int)buf, currentCharacter).ConvertToCoordPath();
        }

        currentCharacter.Skills[skillNo].Target = (Character)Context.Target;
        currentCharacter.ExecuteSkill(skillNo, coordsPath);
    }
}
