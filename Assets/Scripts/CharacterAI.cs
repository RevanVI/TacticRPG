using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : Character, AIAgent
{
    public DecisionMaker DecisionMakerRef;

    public void MakeTurn()
    {
        //calculate decision
        ContextBase context = GetContext();
        DecisionMakerRef.MakeDecisionsList(context);
        Decision decision = DecisionMakerRef.Select();

        //perform decision's action
        decision.QualifierRef.Action.Execute();
    }

    //AIAgent part

    public ContextBase GetContext()
    {
        ContextBase context = new ContextBase();
        context.Provider = this;
        context.AvailableMeleeTargets = GameController.Instance.AvailableMeleeTargets;
        context.AvailableRangedTargets = GameController.Instance.AvailableRangedTargets;
        return context;
    }

    public Character GetControlledCharacter()
    {
        return this;
    }

}
