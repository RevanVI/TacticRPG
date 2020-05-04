using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewQualifier", menuName = "UtilityAI/Qualifier", order = 10)]
public class Qualifier: ScriptableObject
{
    public string Name;

    public ActionBase Action;

    [Serializable]
    public struct ConsiderationPair
    {
        public ConsiderationBase Consideration;
        public AnimationCurve ResolveCurve;
    }

    public List<ConsiderationPair> Considerations;

    public virtual float Score(ContextBase context, float minValue)
    {
        float totalScore = 1f;
        for (int i = 0; i < Considerations.Count; ++i)
        {
            float score = Considerations[i].Consideration.Score(context);
            score = Considerations[i].ResolveCurve.Evaluate(score);

            totalScore *= score;

            //already has worse result
            if (totalScore < minValue)
                return 0f;
        }
        return 0f;
    }

}
