﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeAttackAction", menuName = "UtilityAI/MeleeAttackAction", order = 151)]
public class MeleeAttackAction : ActionBase
{
    public override void Execute()
    {
        Character currentCharacter = Context.Provider.GetControlledCharacter();
        GridSystem.Instance.ResetMovemap();
        List<Node> nodePath = GridSystem.Instance.BuildPath(currentCharacter.Coords, ((Character)Context.Target).Coords, currentCharacter);
        List<Vector3Int> coordsPath = GridSystem.Instance.ConvertFromGraphPath(nodePath);

        currentCharacter.Move(coordsPath, (Character)Context.Target);
    }
}
