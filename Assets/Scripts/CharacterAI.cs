using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAI : Character, AIAgent
{
    public DecisionMaker DecisionMakerRef;
    //AIAgent part

    public ContextBase GetContext()
    {
        ContextBase context = new ContextBase();

        return context;
    }

    public Character GetControlledCharacter()
    {
        return this;
    }

}
