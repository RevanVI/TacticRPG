using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decision
{
    public Qualifier QualifierRef;
    public ContextBase Context;
}

public class Decision<T>: Decision
{
    public T Target;
}

[CreateAssetMenu(fileName = "DecisionMaker", menuName = "UtilityAI/DecisionMaker", order = 1)]
public class DecisionMaker: ScriptableObject
{
    public string Name;
    public AIAgent Requester;
    public List<Qualifier> Qualifiers;
    public List<Decision> PossibleDecisions = null;

    //foreach possible decision pair it with target given by context
    //this is bad solution, because to create new qualifier we need to upgrade this method
    public void MakeDecisionsList(ContextBase context)
    {
        if (PossibleDecisions == null)
            PossibleDecisions = new List<Decision>();
        PossibleDecisions.Clear();
        for (int i = 0; i < Qualifiers.Count; ++i)
        {
            if (Qualifiers[i].Id == UtilityAISystem.Qualifiers.MeleeAttack)
            {
                for (int j = 0; j < context.AvailableMeleeTargets.Count; ++j)
                {
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Target = GameController.Instance.FindCharacter(context.AvailableMeleeTargets[j]);
                    decision.QualifierRef = Qualifiers[i];
                    PossibleDecisions.Add(decision);
                }
            }
            else if (Qualifiers[i].Id == UtilityAISystem.Qualifiers.Move)
            {
                List<Vector3Int> movemap = GridSystem.Instance.GetCurrentMovemap();
                for (int j = 0; j < movemap.Count; ++j)
                {
                    if (GridSystem.Instance.GetNode(movemap[j]).GetCharacter() != null)
                        continue;
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Target = movemap[j];
                    decision.QualifierRef = Qualifiers[i];
                    PossibleDecisions.Add(decision);
                }
            }

        }
    }

    public Decision Select()
    {
        float maxScore = float.MinValue;
        Decision bestDecision = null;
        for (int i = 0; i < PossibleDecisions.Count; ++i)
        {
            float curScore = PossibleDecisions[i].QualifierRef.Score(PossibleDecisions[i].Context, maxScore);
            if (curScore > maxScore)
            {
                maxScore = curScore;
                bestDecision = PossibleDecisions[i];
            }

        }

        bestDecision.QualifierRef.Action.Context = bestDecision.Context;
        return bestDecision;
    }
}
