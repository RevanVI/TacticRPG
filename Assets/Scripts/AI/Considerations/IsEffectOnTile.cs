using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IsEffectOnTile", menuName = "UtilityAI/IsEffectOnTile", order = 101)]
public class IsEffectOnTile : ConsiderationBase
{
    public override float Score(ContextBase context)
    {
        Path path = GridSystem.Instance.BuildPath(context.Provider.GetControlledCharacter().Coords, (Vector3Int)context.Target, context.Provider.GetControlledCharacter());
        if (path != null && !path.IsEffectOnPath)
            return 0f;
        return 1f;
    }
}
