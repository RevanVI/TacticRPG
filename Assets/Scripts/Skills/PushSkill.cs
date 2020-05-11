using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PushSkill", menuName = "Skills/PushSkill", order = 101)]
public class PushSkill : Skill
{
    public int Value;
    public int Time;

    public override void AdditionalChecks(Character target, List<Vector3Int> possiblePositions)
    {
        //we need open tile in possible direction after target
        for (int i = 0; i < possiblePositions.Count; ++i)
        {
            Vector3Int targetMoveCoords = target.Coords + (target.Coords - possiblePositions[i]);
            if (GridSystem.Instance.GetNode(targetMoveCoords).GameStatus != Node.TileGameStatus.Empty)
            {
                possiblePositions.RemoveAt(i);
                --i;
            }
        }
    }

    public override void Execute()
    {
        User.WaitForCallback = true;
        Vector3Int currentCharacterCoords = User.Coords;
        Vector3Int targetCharacterCoords = ((Character)Target).Coords;

        List<Vector3Int> targetPathList = new List<Vector3Int>();
        targetPathList.Add(targetCharacterCoords + (targetCharacterCoords - currentCharacterCoords));

        ((Character)Target).Move(targetPathList, User);

        Effect stunEffect = new Effect();
        stunEffect.Time = Value + 1;
        stunEffect.Type = EffectType.Stun;
        ((Character)Target).ActiveEffects.Add(stunEffect);
        

        base.Execute();
    }
}
