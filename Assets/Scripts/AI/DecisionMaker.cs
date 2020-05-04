using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Decision
{
    public Qualifier QualifierRef;
    public ContextBase Context;
}

public class DecisionMaker
{
    public string Name;
    public AIAgent Requester;
    public List<Qualifier> Qualifiers;
    public List<Decision> PossibleDecisions;

    //foreach possible decision pair it with target given by context
    //this is not ideal solution, because to create new qualifier we need to ipgrade this method
    public List<Decision> MakeDecisionsList(ContextBase context)
    {

        return null;
    }

    public Decision Select()
    {
        return null;
    }
}
