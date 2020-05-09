using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HealSkill", menuName = "Skills/HealSkill", order = 101)]
public class HealSkill : Skill
{
    public int Value;

    public override void Execute()
    {
        //Character targetCharacter = (Character)Target;
        //targetCharacter.AddHP(Value);

        ((Character)Target).TakeDamage(Value);

        if (Cooldown > 0)
        {
            if (CurrentCooldown > 0)
                --CurrentCooldown;
            else
                CurrentCooldown = Cooldown;
        }
        else
            --CurrentCount;
            
        OnExecute.Invoke();
    }
}
