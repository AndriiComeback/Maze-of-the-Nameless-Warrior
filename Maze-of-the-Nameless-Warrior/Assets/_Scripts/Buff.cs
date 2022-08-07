using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buff
{
    private AbilityType type;
    private int turnsLeft;
    public AbilityType Type { set { type = value; } get { return type; } }
    public int TurnsLeft { set {
            turnsLeft = value; 
            if (turnsLeft < 0) {
                turnsLeft = 0;
            }
        } 
        get { return turnsLeft; } 
    }
    public Buff(AbilityType type, int turnsLeft) {
        this.type = type;
        this.turnsLeft = turnsLeft;
    }
}
