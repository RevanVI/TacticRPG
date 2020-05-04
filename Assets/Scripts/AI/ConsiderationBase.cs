using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsiderationBase 
{
    public string Name;
    public AnimationCurve ResolveCurve;

    public virtual float Score(ContextBase context)
    {
        return 0f;
    }

    public virtual float CalculateCurve(float score)
    {
        return ResolveCurve.Evaluate(score);
    }
}
