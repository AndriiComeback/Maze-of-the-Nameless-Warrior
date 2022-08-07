using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroUnit : Unit {
    [SerializeField] private int maxEnergy;
    [SerializeField] private Ability ability;
    private int currentEnergy;
    private Buff currentBuff;
    public int MaxEnergy { set { maxEnergy = value; } get { return maxEnergy; } }
    public int CurrentEnergy { 
        set {
            if (CurrentHealth == 0 || value < 0) {
                return;
            }
            currentEnergy = value;
            if (currentEnergy > MaxEnergy) {
                currentEnergy = MaxEnergy;
            }
        } 
        get { 
            return currentEnergy; 
        } 
    }
    public Ability Ability { set { ability = value; } get { return ability; } }
    public Buff CurrentBuff { private set { currentBuff = value; } get { return currentBuff; } }
    private void Awake() {
        CurrentEnergy = MaxEnergy;
    }
    public override bool TakeDamage(int damage) {
        if (CurrentHealth == 0) {
            return true;
        }
        if (CurrentBuff?.Type == AbilityType.Provoke || CurrentBuff?.Type == AbilityType.Protect) {
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
    public string DecreaseTempEffectsReturnDescription() {
        if (CurrentHealth == 0) {
            return string.Empty;
        }
        if (CurrentBuff != null) {
            CurrentBuff.TurnsLeft--;
            if (CurrentBuff.TurnsLeft == 0) {
                if (CurrentBuff.Type == AbilityType.Provoke) {
                    CurrentBuff = null;
                    return $"As it begins to draw fewer attacks, {UnitName} abandons the Drunk Stance.";
                } else if (CurrentBuff.Type == AbilityType.Protect) {
                    CurrentBuff = null;
                    return $"{UnitName} is no longer protected by smoke.";
                }
                CurrentBuff = null;
            }
        }
        return string.Empty;
    }
    public void GrantBuff(AbilityType type, int duration) {
        if (CurrentBuff != null) {
            if (type == AbilityType.Provoke) {
                currentBuff = new Buff(type, duration);
            } else if (type == AbilityType.Protect) {
                if (CurrentBuff.Type != AbilityType.Provoke) {
                    currentBuff = new Buff(type, duration);
                }
            }
        } else {
            CurrentBuff = new Buff(type, duration);
        }
    }
}