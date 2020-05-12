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
            if (Qualifiers[i].Type == UtilityAISystem.Qualifiers.MeleeAttack)
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
            else if (Qualifiers[i].Type == UtilityAISystem.Qualifiers.Move)
            {
                Character currentCharacter = context.Provider.GetControlledCharacter();
                Movemap movemap = GridSystem.Instance.GetCurrentMovemap();
                for (int j = 0; j < movemap.MoveCoords.Count; ++j)
                {
                    if (movemap.MoveCoords[j] == currentCharacter.Coords)
                        continue;
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Target = movemap.MoveCoords[j];
                    decision.QualifierRef = Qualifiers[i];
                    PossibleDecisions.Add(decision);
                }
            }
            else if (Qualifiers[i].Type == UtilityAISystem.Qualifiers.RangedAttack)
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
            else if (Qualifiers[i].Type == UtilityAISystem.Qualifiers.Skill)
            {
                Character user = context.Provider.GetControlledCharacter();
                Skill skill = user.Skills[Qualifiers[i].SkillNo];
                if (skill.CurrentCooldown > 0 || skill.CurrentCount == 0)
                    continue;
                if (skill.TypeTarget == Skill.TargetType.Self)
                {
                    Decision decision = new Decision();
                    decision.Context = context.Copy();
                    decision.Context.Data.Add("SkillNo", Qualifiers[i].SkillNo);
                    decision.QualifierRef = Qualifiers[i];
                    PossibleDecisions.Add(decision);
                }
                else if (skill.TypeUse == Skill.UseType.Melee)
                {
                    //define list of target fractions
                    List<string> fractionList = new List<string>();
                    if (skill.FractionTarget == Skill.TargetFraction.Enemy)
                        fractionList.Add(user.GetOppositeFraction());
                    else if (skill.FractionTarget == Skill.TargetFraction.Ally)
                        fractionList.Add(user.tag);
                    else
                    {
                        fractionList.Add(user.GetOppositeFraction());
                        fractionList.Add(user.tag);
                    }
                    //define all data
                    Movemap skillMovemap = new Movemap();
                    skillMovemap.MoveCoords.AddRange(GridSystem.Instance.GetCurrentMovemap().MoveCoords);
                    //define all target that character can reach 
                    GridSystem.Instance.DefineAvailableMeleeTargets(skillMovemap, user, GameController.Instance.CharacterList, GridSystem.ConvertFractionsFromStringToNode(fractionList), skill.Distance);

                    //define all positions to attack for all targets
                    List<List<Vector3Int>> possiblePositions = new List<List<Vector3Int>>();
                    for (int j = 0; j < skillMovemap.MeleeCoords.Count; ++j)
                    {
                        Character targetCharacter = GridSystem.Instance.GetCharacterFromCoords(skillMovemap.MeleeCoords[j]);
                        //get possible positions to attack
                        possiblePositions.Add(GridSystem.Instance.DefinePositionsToAttackTarget(skillMovemap, targetCharacter, skill.Distance));
                        //additional special checks
                        skill.AdditionalChecks(targetCharacter, possiblePositions[j]);
                        //if there no possible positions to attack than delete this target from list
                        if (possiblePositions[j].Count == 0)
                        {
                            skillMovemap.MeleeCoords.RemoveAt(j);
                            possiblePositions.RemoveAt(j);
                            --j;
                        }
                    }

                    //create decision to all pairs target-possible position
                    for (int j = 0; j < skillMovemap.MeleeCoords.Count; ++j)
                    {
                        foreach (var coords in possiblePositions[j])
                        {
                            Decision decision = new Decision();
                            decision.Context = context.Copy();
                            decision.Context.Target = GridSystem.Instance.GetCharacterFromCoords(skillMovemap.MeleeCoords[j]);
                            decision.Context.Data.Add("AttackTile", coords);
                            decision.Context.Data.Add("SkillNo", Qualifiers[i].SkillNo);
                            decision.QualifierRef = Qualifiers[i];
                            PossibleDecisions.Add(decision);
                        }
                    }
                }
                else if (skill.TypeUse == Skill.UseType.Randged)
                {
                    //define list of target fractions
                    List<string> fractionList = new List<string>();
                    if (skill.FractionTarget == Skill.TargetFraction.Enemy)
                        fractionList.Add(user.GetOppositeFraction());
                    else if (skill.FractionTarget == Skill.TargetFraction.Ally)
                        fractionList.Add(user.tag);
                    else
                    {
                        fractionList.Add(user.GetOppositeFraction());
                        fractionList.Add(user.tag);
                    }
                                                   
                    Movemap skillMovemap = new Movemap();
                    bool isThereEnemyNearby = GameController.Instance.IsThereEnemyNearby(user);

                    if ((isThereEnemyNearby && skill.UseNearEnemy) ||
                        (!isThereEnemyNearby && user.Properties.Class != CharacterClass.Archer))
                    {
                        foreach (var fraction in fractionList)
                            skillMovemap.RangeCoords.AddRange(GameController.Instance.DefineAvailableRangedTargets(user, fraction));
                    }
                    else if (!isThereEnemyNearby && user.Properties.Class == CharacterClass.Archer) //this can reduce amount of raycasts
                    {
                        skillMovemap.RangeCoords.AddRange(GridSystem.Instance.GetCurrentMovemap().RangeCoords);
                        if (fractionList.Contains(user.tag))
                            skillMovemap.RangeCoords.AddRange(GameController.Instance.DefineAvailableRangedTargets(user, user.tag));
                    }

                    foreach (var targetCoords in skillMovemap.RangeCoords)
                    {
                        Decision decision = new Decision();
                        decision.Context = context.Copy();
                        decision.Context.Target = GridSystem.Instance.GetCharacterFromCoords(targetCoords);
                        decision.Context.Data.Add("SkillNo", Qualifiers[i].SkillNo);
                        decision.QualifierRef = Qualifiers[i];
                        PossibleDecisions.Add(decision);
                    }
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
