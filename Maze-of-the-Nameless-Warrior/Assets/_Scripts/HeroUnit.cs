using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUnit : Unit {
    [field: SerializeField] public int MaxEnergy { private set; get; }
    [field: SerializeField] public int CurrentEnergy { set; get; } // to do method
    [field: SerializeField] public Ability Ability { private set; get; }
    public int ProtectionRoundsLeft { set; get; }
    public int PriorityStatusRoundsLeft { set; get; }
    private void Start() {
        CurrentEnergy = MaxEnergy;
    }
    public override bool TakeDamage(int damage) {
        if (ProtectionRoundsLeft > 0) {
            damage = damage / 2;
        }
        return base.TakeDamage(damage);
    }
    public void Meditate(int amount) {
        CurrentEnergy += amount;
        if (CurrentEnergy > MaxEnergy) {
            CurrentEnergy = MaxEnergy;
        }
    }
    public bool DecreaseProtection() {
        ProtectionRoundsLeft--;
        if (ProtectionRoundsLeft == 0) {
            return true;
        } else if (ProtectionRoundsLeft < 0) {
            ProtectionRoundsLeft = 0;
        }
        return false;
    }
    public bool DecreasePriority() {
        PriorityStatusRoundsLeft--;
        if (PriorityStatusRoundsLeft == 0) {
            return true;
        } else if (PriorityStatusRoundsLeft < 0) {
            PriorityStatusRoundsLeft = 0;
        }
        return false;
    }
}
