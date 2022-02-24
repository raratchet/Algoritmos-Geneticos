using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Actions
{
    ATTACK, HEAL, RELOAD, RETREAT, NONE
}

public enum State
{
    HI, MED, LOW
}

public class Fuzzifier
{
    static Fuzzifier instance  = null;

     public static Fuzzifier Instance { get 
        { if (instance == null) instance = new Fuzzifier();
            return instance;
        } }

    public Actions GetAction (float health, int ammo,Chromosome chromo)
    {
        float health_n = health / 100;
        float ammo_n = ammo / 30;

        State hst = chromo.GetState("HEALTH",health_n);
        State ast = chromo.GetState("AMMO",ammo_n);


        Actions action = Actions.NONE;

        if (hst == State.HI && ast == State.HI)
            action = Actions.ATTACK;
        else if (hst == State.HI && ast == State.MED)
            action = Actions.ATTACK;
        else if (hst == State.HI && ast == State.LOW)
            action = Actions.RELOAD;
        else if (hst == State.MED && ast == State.HI)
            action = Actions.ATTACK;
        else if (hst == State.MED && ast == State.MED)
            action = Actions.HEAL;
        else if (hst == State.MED && ast == State.LOW)
            action = Actions.RELOAD;
        else if (hst == State.LOW && ast == State.HI)
            action = Actions.HEAL;
        else if (hst == State.LOW && ast == State.MED)
            action = Actions.ATTACK;
        else if (hst == State.LOW && ast == State.LOW)
            action = Actions.HEAL;


        return action;
    }
}
