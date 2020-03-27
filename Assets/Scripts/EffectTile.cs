﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    Damage = 0,
    Heal = 1,
    Stun = 2,
    Slow = 3,
}


public class EffectTile : MonoBehaviour
{
    /*
     * Defines how long effect tile will live
     * -1 - infinite (for example, lava or trap)
     *  0 - dicrete 
     * >0 - turn counts (for example, flame or electrified water)
     */
    public int Lifetime;

    public EffectType Type;
    
    //calls then effect placed on tile
    public void StartEffect()
    {

    }

    //calls then effect remove from tile
    public void EndEffect()
    {
        Debug.Log("Effect destroyed");
        Destroy(this);
    }

    void EndTurn()
    {
        if (Lifetime > 0)
        {
            --Lifetime;
            if (Lifetime == 0)
            {
                GridSystem.Instance.RemoveEffect(this);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GridSystem.Instance.DefineEffect(this);
        GameController.Instance.OnTurnEnd.AddListener(EndTurn);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Someone has entered in collider");
        //do something
    }
}
