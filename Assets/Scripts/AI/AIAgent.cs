using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface AIAgent
{
    Character GetControlledCharacter();

    ContextBase GetContext();
}
