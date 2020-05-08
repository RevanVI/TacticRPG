using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Skill: ScriptableObject
{
    public string Name;
    public Sprite SkillImage;
    public int Cooldown;
    public int Count;
    public int CurrentCount;
    public object Target;

    public UnityEvent OnExecute = new UnityEvent();

    public virtual void Execute()
    {

    }
}

[CreateAssetMenu(fileName = "HealSkill", menuName = "Skills/HealSkill", order = 101)]
public class HealSkill: Skill
{
    public int Value;

    public override void Execute()
    {
        Character targetCharacter = (Character)Target;
        targetCharacter.AddHP(Value);
        OnExecute.Invoke();
    }
}

