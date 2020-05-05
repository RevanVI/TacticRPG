using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ConsiderationBase: ScriptableObject
{
    public string Name;

    public virtual float Score(ContextBase context)
    {
        return 0f;
    }
}
