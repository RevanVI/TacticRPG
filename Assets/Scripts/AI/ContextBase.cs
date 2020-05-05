using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextBase
{
    public AIAgent Provider;
    public object Target;

    public List<Vector3Int> AvailableRangedTargets;
    public List<Vector3Int> AvailableMeleeTargets;

    public ContextBase Copy()
    {
        ContextBase context = new ContextBase();
        context.Provider = Provider;
        context.AvailableMeleeTargets = AvailableMeleeTargets;
        context.AvailableRangedTargets = AvailableRangedTargets;
        return context;
    }
}
