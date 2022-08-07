using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InitiativeHelper {
    List<HeroUnit> heroes;
    Unit enemy;
    List<InitiativeIndex> inits;
    int currentUnitIndex;
    int round;
    public InitiativeHelper(List<HeroUnit> heroes, Unit enemy) {
        inits = new List<InitiativeIndex>();
        round = 0;
        this.heroes = heroes;
        this.enemy = enemy;
        RollInitiative();
    }
    void RollInitiative() {
        round++;
        inits.Clear();
        for (int i = 0; i < heroes.Count; i++) {
            if (heroes[i].CurrentHealth > 0) {
                int init = Random.Range(1, 21) + heroes[i].InitiativeModifier;
                inits.Add(new InitiativeIndex { index = i, initiative = init });
            }
        }
        inits.Add(new InitiativeIndex { index = 3, initiative = Random.Range(1, 21) + enemy.InitiativeModifier });
        inits = inits.OrderByDescending(x => x.initiative).ToList();
        currentUnitIndex = inits[0].index;
    }
    public int GetCurrentUnitIndex() {
        return currentUnitIndex;
    }
    public bool MoveTurnOrder() {
        inits.RemoveAll(x => x.index == currentUnitIndex);
        if (inits.Count > 0) {
            currentUnitIndex = inits[0].index;
            if (currentUnitIndex != 3) {
                if (heroes[currentUnitIndex].CurrentHealth == 0) {
                    MoveTurnOrder();
                }
            }
            return false;
        } else {
            RollInitiative();
            return true;
        }
    }
    public bool RemovePlayer(int index) {
        inits.RemoveAll(x => x.index == index);
        return heroes[0].CurrentHealth == 0 && heroes[1].CurrentHealth == 0 && heroes[2].CurrentHealth == 0;
    }
}

public struct InitiativeIndex {
    public int initiative;
    public int index;
}
