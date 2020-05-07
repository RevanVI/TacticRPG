using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PathLenght", menuName = "UtilityAI/PathLenght", order = 101)]
public class PathLenght : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Character currentCharacter = context.Provider.GetControlledCharacter();
        Vector3Int attackTileCoords = (Vector3Int)context.Data["AttackTile"];

        Path path = GridSystem.Instance.BuildPath(currentCharacter.Coords, attackTileCoords, currentCharacter);

        return ((float)path.NodePath.Count) / currentCharacter.Properties.Speed;
    }
}
