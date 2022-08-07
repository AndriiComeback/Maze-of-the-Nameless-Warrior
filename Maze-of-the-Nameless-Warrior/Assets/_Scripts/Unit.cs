using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour 
{
    [field: SerializeField] public string UnitName { private set; get; }
    [field: SerializeField] public int MaxHealth { set; get; }
    [field: SerializeField] public int CurrentHealth { private set; get; }
    [field: SerializeField] public int DamageMin { set; get; }
    [field: SerializeField] public int DamageMax { set; get; }
    [field: SerializeField] public int InitiativeModifier { set; get; }
    public virtual bool TakeDamage(int damage) {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0) {
            CurrentHealth = 0;
            return true;
        }
        return false;
    }
    public void Heal(int amount) {
        if (CurrentHealth == 0) {
            return;
        }
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth) {
            CurrentHealth = MaxHealth;
        }
    }
}