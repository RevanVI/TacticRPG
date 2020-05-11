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
            Vector3Int targetMoveCoords = target.Coords + (possiblePositions[i] - target.Coords);
            if (GridSystem.Instance.GetNode(targetMoveCoords).GameStatus != Node.TileGameStatus.Empty)
            {
                possiblePositions.RemoveAt(i);
                --i;
            }
        }
    }

    public override void Execute()
    {
        

        base.Execute();
    }
}
