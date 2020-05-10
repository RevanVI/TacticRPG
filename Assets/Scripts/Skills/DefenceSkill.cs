using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DefenceSkill", menuName = "Skills/DefenceSkill", order = 101)]
public class DefenceSkill : Skill
{
    [Range(0, 1)]
    [Tooltip("Amount damage to absorb")]
    public float Value;
    public int Time;

    public override void Execute()
    {
        Effect effect = new Effect();
        effect.Type = EffectType.Defence;
        effect.Value = Value;
        effect.Time = Time;

        ((Character)Target).ActiveEffects.Add(effect);

        base.Execute();
    }
}
