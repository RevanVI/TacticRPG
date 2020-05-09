using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decision
{
    public QualifierElement QualifierRef;
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

    public List<QualifierElement> Qualifiers;
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
                    Character target = GameController.Instance.FindCharacter(context.AvailableMeleeTargets[j]);
                    //we need define from that side character can attack his target
                    List<Vector3Int> possibleTilesToAttack = GridSystem.Instance.GetNearMovemapTilesList(target.Coords);
                    if (GameController.Instance.IsCharactersStayNear(context.Provider.GetControlledCharacter(), target))
                        possibleTilesToAttack.Add(context.Provider.GetControlledCharacter().Coords);
                    foreach (var coords in possibleTilesToAttack)
                    {
                        Decision decision = new Decision();
                        decision.Context = context.Copy();
                        decision.Context.Target = target;
                        decision.Context.Data.Add("AttackTile", coords);
                        decision.QualifierRef = Qualifiers[i];
                        PossibleDecisions.Add(decision);
                    }
                }
            }
            else if (Qualifiers[i].Id == UtilityAISystem.Qualifiers.Move)
            {
                Movemap movemap = GridSystem.Instance.GetCurrentMovemap();
                for (int j = 0; j < movemap.MoveCoords.Count; ++j)
                {
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Target = movemap.MoveCoords[j];
                    decision.QualifierRef = Qualifiers[i];
                    PossibleDecisions.Add(decision);
                }
            }
            else if (Qualifiers[i].Id == UtilityAISystem.Qualifiers.RangedAttack)
            {
                for (int j = 0; j < context.AvailableRangedTargets.Count; ++j)
                {
                    Character target = GameController.Instance.FindCharacter(context.AvailableRangedTargets[j]);
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Target = target;
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

        LogHandler.WriteText($"Turn: {GameController.Instance.TurnCount}\nCharacter: {GameController.Instance._currentCharacter.name}\n");

        for (int i = 0; i < PossibleDecisions.Count; ++i)
        {
            LogHandler.WriteText($"\tDCE: {PossibleDecisions[i].QualifierRef.Name}");

            float score = PossibleDecisions[i].QualifierRef.Score(PossibleDecisions[i].Context, maxScore);
            if (score > maxScore)
            {
                maxScore = score;
                bestDecision = PossibleDecisions[i];
            }

        }

        bestDecision.QualifierRef.Action.Context = bestDecision.Context;
        return bestDecision;
    }
}
