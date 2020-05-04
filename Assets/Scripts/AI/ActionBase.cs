using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionBase
{
    public int ActionId;
    public ContextBase Context;

    public virtual void Execute()
    {
        return;
    }
}
