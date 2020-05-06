using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "MoveAction", menuName = "UtilityAI/MoveAction", order = 151)]
public class MoveAction : ActionBase
{
    public override void Execute()
    {
        Character currentCharacter = Context.Provider.GetControlledCharacter();
        GridSystem.Instance.ResetMovemap();
        Path path = GridSystem.Instance.BuildPath(currentCharacter.Coords, (Vector3Int)Context.Target, currentCharacter);
        List<Vector3Int> coordsPath = path.ConvertToCoordPath();

        currentCharacter.Move(coordsPath);
    }
}
