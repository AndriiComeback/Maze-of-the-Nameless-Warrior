using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour 
{
    [field: SerializeField] public string UnitName { private set; get; }
    [field: SerializeField] public int MaxHealth { private set; get; }
    [field: SerializeField] public int CurrentHealth { private set; get; }
    [field: SerializeField] public int DamageMin { private set; get; }
    [field: SerializeField] public int DamageMax { private set; get; }
    [field: SerializeField] public int InitiativeModifier { private set; get; }
    public virtual bool TakeDamage(int damage) {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0) {
            CurrentHealth = 0;
            return true;
        }
        return false;
    }
    public void Heal(int amount) {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) {
            CurrentHealth = MaxHealth;
        }
    }
}