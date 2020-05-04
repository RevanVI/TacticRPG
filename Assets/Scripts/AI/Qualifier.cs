using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Qualifier
{
    public string Name;

    public ActionBase Action;
    public List<ConsiderationBase> Considerations;

    public virtual List<ActionBase> Score(ContextBase context, float minValue)
    {
        //for every inntro in context go through all cinsiderations and calculate score

        return null;
    }

}
