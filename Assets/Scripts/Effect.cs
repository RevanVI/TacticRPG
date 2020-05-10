using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Defence = 0,
    Heal = 1,
    Damage = 2,
}

public class Effect
{
    public EffectType Type;
    public int Time;

    public object Value;

    public virtual void Process()
    {
        --Time;
    }
}
