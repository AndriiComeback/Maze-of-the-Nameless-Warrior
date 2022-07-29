using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { PlayerTurn, PassingTurn, EnemyTurn, Won, Lost, EnemyDefeated, Fleed }

public class BattleSystem : MonoBehaviour
{
    private BattleState state;
    [SerializeField] private BattleHUD battleHUD;
    [SerializeField, Range(0, 100)] int fleeChance = 33;

    List<HeroUnit> heroes;
    List<EnemyUnit> enemies;

    HeroUnit currentHero;
    EnemyUnit currentEnemy;

    InitiativeHelper initHelper;
    int round;

    #region Methods: protected

    protected IEnumerator HandleNextEnemy() {
        enemies.Remove(currentEnemy);
        if (enemies.Count == 0) {
            yield return new WaitForSeconds(1f);

            battleHUD.WriteLog("Loot! To do.");

            state = BattleState.Won;
        } else {
            yield return new WaitForSeconds(1f);

            currentEnemy = enemies[0];
            initHelper = new InitiativeHelper(heroes, currentEnemy);

            battleHUD.SetEnemyImage(currentEnemy.Sprite);
            battleHUD.WriteLog(currentEnemy.BattleDescription);
            battleHUD.SetHUD(heroes, currentEnemy, false);

            state = BattleState.PassingTurn;
            HandleTurn(true);
        }
    }
    protected void HandleTurn(bool isNewEnemy = false) {
        if (state != BattleState.PassingTurn) {
            return;
        }
        bool isNewRound = true;
        if (!isNewEnemy) {
            isNewRound = initHelper.MoveTurnOrder();
        }
        if (isNewRound) {
            battleHUD.WriteLog($"Round {round++}");
        }

        battleHUD.ClearHighlight();
        int index = initHelper.GetCurrentUnitIndex();
        if (index == 3) {
            state = BattleState.EnemyTurn;
            StartCoroutine(EnemyTurn());
        } else if (index >= 0 && index <= 2) {
            HeroUnit currentHero = heroes[index];
            state = BattleState.PlayerTurn;
            PlayerTurn(currentHero, index);
        }
    }
    protected void PlayerTurn(HeroUnit currentHero, int index) {
        bool doesProtectionjustEnded = currentHero.DecreaseProtection();
        bool doesPriorityjustEnded = currentHero.DecreasePriority();
        if (doesProtectionjustEnded && !doesPriorityjustEnded) {
            battleHUD.WriteLog($"{currentHero.UnitName} is no longer protected.");
        }
        if (doesPriorityjustEnded) {
            battleHUD.WriteLog($"As it begins to draw fewer attacks, {currentHero.UnitName} abandons the Drunk Stance.");
        }

        battleHUD.SetAbilityButtonText(currentHero.Ability.Title);
        bool isEnoughEnergy = currentHero.CurrentEnergy >= currentHero.Ability.Cost;
        battleHUD.SetAbilityButtonTextActive(isEnoughEnergy);

        battleHUD.HighlightPlayer(index);
    }
    protected IEnumerator EnemyTurn() {
        yield return new WaitForSeconds(1f);
        int targetIndex = heroes.FindIndex(x => x.PriorityStatusRoundsLeft > 0);
        if (targetIndex < 0) {
            targetIndex = Random.Range(0, 3);
        }
        int damageRolled = Random.Range(currentEnemy.DamageMin, currentEnemy.DamageMax + 1);
        bool isDead = heroes[targetIndex].TakeDamage(damageRolled);
        battleHUD.SetHeroHP(targetIndex, heroes[targetIndex].CurrentHealth);
        battleHUD.WriteLog($"{currentEnemy.UnitName} hits {heroes[targetIndex].UnitName} for {damageRolled} damage.");
        if (heroes[targetIndex].ProtectionRoundsLeft > 0) {
            battleHUD.WriteLog($"{heroes[targetIndex].UnitName} is protected and takes only {damageRolled / 2} damage.");
        }
        bool isAllDead = false;
        if (isDead) {
            isAllDead = initHelper.RemovePlayer(targetIndex);
        }
        if (isAllDead) {
            state = BattleState.Lost;
            battleHUD.WriteLog("Lost");
        } else {
            state = BattleState.PassingTurn;
            HandleTurn();
        }
    } 

    #endregion

    #region Methods: public

    public IEnumerator StartNewBattle(List<HeroUnit> heroes, List<EnemyUnit> enemies) {
        this.heroes = heroes;
        this.enemies = enemies;
        currentEnemy = enemies[0];
        initHelper = new InitiativeHelper(heroes, currentEnemy);
        round = 1;

        state = BattleState.PassingTurn;
        battleHUD.SetHUD(heroes, currentEnemy);
        battleHUD.SetEnemyImage(currentEnemy.Sprite);


        yield return new WaitForSeconds(1f);
        battleHUD.WriteLog(currentEnemy.BattleDescription);

        yield return new WaitForSeconds(1f);
        HandleTurn(true);
    }

    #region Button click actions

    public void OnAttackButton() {
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        StartCoroutine(PlayerAttack(1));
        state = BattleState.PassingTurn;
    }
    public void OnUseAbilityButton() {
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        StartCoroutine(PlayerUseAbility());
        state = BattleState.PassingTurn;
    }
    public void OnMeditateButton() {
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        StartCoroutine(PlayerMeditate());
        state = BattleState.PassingTurn;
    }
    public void OnRecoverButton() {
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        StartCoroutine(PlayerRecover());
        state = BattleState.PassingTurn;
    }
    public void OnFleeButton() {
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        StartCoroutine(PlayerFlee());
        state = BattleState.PassingTurn;
    }

    #endregion

    #region Player actions

    IEnumerator PlayerAttack(int attacksCount) {
        yield return new WaitForSeconds(1f);
        int index = initHelper.GetCurrentUnitIndex();
        bool isDead = false;
        for (int i = 0; i < attacksCount; i++) {
            int damageRolled = Random.Range(heroes[index].DamageMin, heroes[index].DamageMax + 1);
            isDead = currentEnemy.TakeDamage(damageRolled);
            
            battleHUD.SetEnemyHP(currentEnemy.CurrentHealth);
            battleHUD.WriteLog($"{heroes[index].UnitName} hits {currentEnemy.UnitName} for {damageRolled} damage.");
            if (isDead) {
                state = BattleState.EnemyDefeated;
                battleHUD.WriteLog($"{currentEnemy.UnitName} is dead.");
                i = attacksCount;
                StartCoroutine(HandleNextEnemy());
            }
            yield return new WaitForSeconds(1f);
        }
        if (!isDead) {
            state = BattleState.PassingTurn;
            HandleTurn();
        }
    }
    IEnumerator PlayerUseAbility() {
        yield return new WaitForSeconds(1f);
        int index = initHelper.GetCurrentUnitIndex();
        switch (heroes[index].Ability.Type) {
            case AbilityType.Burst:
                heroes[index].CurrentEnergy -= heroes[index].Ability.Cost;
                battleHUD.SetHeroEnergy(index, heroes[index].CurrentEnergy);
                StartCoroutine(PlayerAttack(3));
                break;
            case AbilityType.Provoke:
                heroes[index].CurrentEnergy -= heroes[index].Ability.Cost;
                battleHUD.SetHeroEnergy(index, heroes[index].CurrentEnergy);
                StartCoroutine(PlayerProvoke());
                break;
            case AbilityType.Protect:
                heroes[index].CurrentEnergy -= heroes[index].Ability.Cost;
                battleHUD.SetHeroEnergy(index, heroes[index].CurrentEnergy);
                StartCoroutine(PlayerProtect());
                break;
            default:
                break;
        }
    }
    IEnumerator PlayerMeditate() {
        yield return new WaitForSeconds(1f);
        int index = initHelper.GetCurrentUnitIndex();
        heroes[index].ProtectionRoundsLeft = 1;
        int prevEnergy = heroes[index].CurrentEnergy;
        int amountRolled = 2;
        heroes[index].Meditate(amountRolled);
        int energy = heroes[index].CurrentEnergy;

        battleHUD.SetHeroEnergy(index, heroes[index].CurrentEnergy);
        battleHUD.WriteLog($"{heroes[index].UnitName} meditates and restores {energy - prevEnergy} energy.");

        state = BattleState.PassingTurn;
        HandleTurn();
    }
    IEnumerator PlayerRecover() {
        yield return new WaitForSeconds(1f);
        int index = initHelper.GetCurrentUnitIndex();
        int prevHealth = heroes[index].CurrentHealth;
        int healRolled = (int)(Random.Range(20, 41) / 100f * heroes[index].MaxHealth);
        heroes[index].Heal(healRolled);
        int health = heroes[index].CurrentHealth;

        battleHUD.SetHeroHP(index, heroes[index].CurrentHealth);
        battleHUD.WriteLog($"{heroes[index].UnitName} bandages the wounds and gains {health - prevHealth} HP.");

        state = BattleState.PassingTurn;
        
        HandleTurn();
    }
    IEnumerator PlayerFlee() {
        yield return new WaitForSeconds(1f);
        int roll = Random.Range(0, 101);
        if (roll <= fleeChance) {
            state = BattleState.Fleed;
            battleHUD.WriteLog("Fleed succeeded!");
        } else {
            battleHUD.WriteLog("Fleed unseccessful!");
            HandleTurn();
        }
    }
    IEnumerator PlayerProvoke() {
        yield return new WaitForSeconds(0.5f);
        int index = initHelper.GetCurrentUnitIndex();
        heroes[index].PriorityStatusRoundsLeft = 2;
        heroes[index].ProtectionRoundsLeft = 2;
        battleHUD.WriteLog($"{heroes[index].UnitName} moves into Drunk Stance and attacks, making him an easy but well-protected target!");

        state = BattleState.PassingTurn;
        HandleTurn();
    }
    IEnumerator PlayerProtect() {
        yield return new WaitForSeconds(0.5f);
        int index = initHelper.GetCurrentUnitIndex();
        for (int i = 0; i < heroes.Count; i++) {
            heroes[i].ProtectionRoundsLeft = 2;
        }
        battleHUD.WriteLog($"{heroes[index].UnitName} protect all allies.");

        state = BattleState.PassingTurn;
        HandleTurn();
    }

    #endregion

    #endregion
}