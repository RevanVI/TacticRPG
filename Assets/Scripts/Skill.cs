using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Skill: ScriptableObject
{
    public enum TargetType
    {
        Character = 0,
        Zone = 1,
        Tile = 2,
        Self = 3,
    }

    public enum UseType
    {
        Melee = 0,
        Randged = 1,
    }

    public enum TargetFraction
    {
        All = 0,
        Ally = 1,
        Enemy = 2,
    }

    public string Name;
    public Sprite SkillImage;

    public UseType TypeUse;
    public int Distance;

    public int Cooldown;
    public int CurrentCooldown;
    public int Count;
    public int CurrentCount;

    public TargetType TypeTarget;
    public TargetFraction FractionTarget;
    public object Target;

    public bool UseNearEnemy;

    public UnityEvent OnExecute = new UnityEvent();

    public virtual void Reset()
    {
        CurrentCooldown = 0;
        CurrentCount = 0;
    }

    public virtual void ProcessTurn()
    {
        if (CurrentCooldown > 0)
            --CurrentCooldown;
    }

    public virtual void Execute()
    {
        if (Cooldown > 0)
            CurrentCooldown = Cooldown + 1;
        else
            --CurrentCount;
        OnExecute.Invoke();
    }

    public virtual void AdditionalChecks(Character target, List<Vector3Int> possiblePositions)
    {

    }
}

