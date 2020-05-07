using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackAction", menuName = "UtilityAI/MeleeAttackAction", order = 151)]
public class MeleeAttackAction : ActionBase
{
    public override void Execute()
    {
        Character currentCharacter = Context.Provider.GetControlledCharacter();
        GridSystem.Instance.ResetMovemap();

        Vector3Int attackTileCoords = (Vector3Int)Context.Data["AttackTile"];

        Path path = GridSystem.Instance.BuildPath(currentCharacter.Coords, attackTileCoords, currentCharacter);
        List<Vector3Int> coordsPath = path.ConvertToCoordPath();
        coordsPath.Add(((Character)Context.Target).Coords);
        currentCharacter.Move(coordsPath, (Character)Context.Target);
    }
}
