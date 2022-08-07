using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BattleState { None, PlayerTurn, PassingTurn, EnemyTurn, Lost, EnemyDefeated, Fleed, Won, LootDistribution, ReadyForLeave }

public class BattleSystem : MonoBehaviour
{
    private BattleState state;
    [SerializeField] GameController gameController;
    [SerializeField] private BattleHUD battleHUD;
    [SerializeField, Range(0, 100)] private int _fleeChance = 33;
    [SerializeField] private const int _energyRegenPerRound = 1;
    [SerializeField] private const int _provokeLength = 4;
    [SerializeField] private const int _protectLength = 3;
    [SerializeField] private const int _meditateLength = 1;

    List<HeroUnit> heroes = new List<HeroUnit>();
    List<EnemyUnit> enemies = new List<EnemyUnit>();

    HeroUnit currentHero;
    EnemyUnit currentEnemy;

    InitiativeHelper initHelper;
    int round;
    Lair lair;
    Item item;

    #region Methods: protected

    protected IEnumerator HandleNextEnemy() {
        enemies.Remove(currentEnemy);
        if (enemies.Count == 0) {

            state = BattleState.Won;
            yield return StartCoroutine(battleHUD.WriteLog($"You've won the battle!"));
            battleHUD.SwitchToSearch(lair.liarImage);
        } else {
            currentEnemy = enemies[0];
            initHelper = new InitiativeHelper(heroes, currentEnemy);

            battleHUD.SetEnemyImage(currentEnemy.Sprite);
            battleHUD.SetHUD(heroes, currentEnemy, false);
            yield return StartCoroutine(battleHUD.WriteLog(currentEnemy.BattleDescription));

            state = BattleState.PassingTurn;
            StartCoroutine(HandleTurn(true));
        }
    }
    protected IEnumerator HandleTurn(bool isNewEnemy = false) {
        if (state != BattleState.PassingTurn) {
            yield break;
        }
        battleHUD.ClearHighlight();
        bool isNewRound = true;
        if (!isNewEnemy) {
            isNewRound = initHelper.MoveTurnOrder();
        }
        if (isNewRound) {
            yield return StartCoroutine(battleHUD.WriteLog($"Round {round++}"));
            HandleNewRoundStaff();
        }
        int index = initHelper.GetCurrentUnitIndex();
        if (index == 3) {
            state = BattleState.EnemyTurn;
            StartCoroutine(EnemyTurn());
        } else if (index >= 0 && index <= 2) {
            HeroUnit currentHero = heroes[index];
            state = BattleState.PlayerTurn;
            StartCoroutine(PlayerTurn(currentHero, index));
        }
    }
    protected void HandleNewRoundStaff() {
        for (int i = 0; i < heroes.Count; i++) {
            heroes[i].CurrentEnergy += _energyRegenPerRound;
            battleHUD.SetHeroEnergy(i, heroes[i].CurrentEnergy);
        }
    }
    protected IEnumerator PlayerTurn(HeroUnit currentHero, int index) {
        battleHUD.SetHeroEnergy(index, currentHero.CurrentEnergy);
        string description = currentHero.DecreaseTempEffectsReturnDescription();
        if (!string.IsNullOrWhiteSpace(description)) {
            yield return StartCoroutine(battleHUD.WriteLog(description));
        }

        battleHUD.SetAbilityButton(currentHero.Ability.Title, currentHero.CurrentEnergy >= currentHero.Ability.Cost);
        battleHUD.HighlightPlayer(index);
    }
    protected IEnumerator EnemyTurn() {
        int targetIndex = heroes.FindIndex(x => x.CurrentBuff?.Type == AbilityType.Provoke && x.CurrentHealth > 0);
        if (targetIndex < 0) {
            int heroesAlive = heroes.FindAll(x => x.CurrentHealth > 0).Count();
            if (heroesAlive == 3) {
                targetIndex = Random.Range(0, heroesAlive);
            } else if (heroesAlive == 2) {
                for (int i = 0; i < 1000; i++) {
                    targetIndex = Random.Range(0, heroes.Count);
                    if (heroes[targetIndex].CurrentHealth > 0) {
                        break;
                    }
                }
            } else if (heroesAlive == 1) {
                targetIndex = heroes.FindIndex(x => x.CurrentHealth > 0);
            }
        }
        int damageRolled = Random.Range(currentEnemy.DamageMin, currentEnemy.DamageMax + 1);
        bool isDead = heroes[targetIndex].TakeDamage(damageRolled);
        battleHUD.FlashHUD();
        battleHUD.SetHeroHP(targetIndex, heroes[targetIndex].CurrentHealth);
        yield return StartCoroutine(battleHUD.WriteLog($"{currentEnemy.UnitName} hits {heroes[targetIndex].UnitName} for {damageRolled} damage."));
        if (heroes[targetIndex].CurrentBuff?.Type == AbilityType.Protect) {
            yield return StartCoroutine(battleHUD.WriteLog($"{heroes[targetIndex].UnitName} is hidden in the smoke and takes only {damageRolled / 2} damage."));
        }
        if (heroes[targetIndex].CurrentBuff?.Type == AbilityType.Provoke) {
            yield return StartCoroutine(battleHUD.WriteLog($"{heroes[targetIndex].UnitName} is in the Drunk Stance and takes only {damageRolled / 2} damage."));
        }
        bool isAllDead = false;
        if (isDead) {
            isAllDead = initHelper.RemovePlayer(targetIndex);
        }
        if (isAllDead) {
            state = BattleState.Lost;
            yield return StartCoroutine(battleHUD.WriteLog("You've lost the battle. Press R to restart."));
            gameController.EndBattle(BattleState.Lost);

        } else {
            state = BattleState.PassingTurn;
            StartCoroutine(HandleTurn());
        }
    } 

    #endregion

    #region Methods: public

    public IEnumerator StartNewBattle(List<HeroUnit> party, List<EnemyUnit> enemies, Lair lair) {
        this.lair = lair;
        heroes = party;
        this.enemies = enemies;
        currentEnemy = enemies[0];
        initHelper = new InitiativeHelper(heroes, currentEnemy);
        round = 1;

        state = BattleState.PassingTurn;
        battleHUD.SetHUD(heroes, currentEnemy);
        battleHUD.SetEnemyImage(currentEnemy.Sprite);

        yield return StartCoroutine(battleHUD.WriteLog(lair.enemyCountDescription));
        yield return StartCoroutine(battleHUD.WriteLog(currentEnemy.BattleDescription));

        StartCoroutine(HandleTurn(true));
    }

    #region Button click actions

    public void OnAttackButton() {
        battleHUD.ClearHighlight();
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        state = BattleState.PassingTurn;
        StartCoroutine(PlayerAttack(1));
    }
    public void OnUseAbilityButton() {
        battleHUD.ClearHighlight();
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        state = BattleState.PassingTurn;
        StartCoroutine(PlayerUseAbility());
    }
    public void OnMeditateButton() {
        battleHUD.ClearHighlight();
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        state = BattleState.PassingTurn;
        StartCoroutine(PlayerMeditate());
    }
    public void OnRecoverButton() {
        battleHUD.ClearHighlight();
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        state = BattleState.PassingTurn;
        StartCoroutine(PlayerRecover());
    }
    public void OnFleeButton() {
        battleHUD.ClearHighlight();
        if (!Equals(state, BattleState.PlayerTurn)) {
            return;
        }
        state = BattleState.PassingTurn;
        StartCoroutine(PlayerFlee());
    }

    public void OnSearchButton() {
        if (!Equals(state, BattleState.Won)) {
            return;
        }
        state = BattleState.LootDistribution;
        StartCoroutine(PlayerSearch());
    }

    public void OnChooseHero1Button() {
        if (!Equals(state, BattleState.LootDistribution)) {
            return;
        }
        state = BattleState.ReadyForLeave;
        StartCoroutine(PlayerChooseHero(0));
    }
    public void OnChooseHero2Button() {
        if (!Equals(state, BattleState.LootDistribution)) {
            return;
        }
        state = BattleState.ReadyForLeave;
        StartCoroutine(PlayerChooseHero(1));
    }
    public void OnChooseHero3Button() {
        if (!Equals(state, BattleState.LootDistribution)) {
            return;
        }
        state = BattleState.ReadyForLeave;
        StartCoroutine(PlayerChooseHero(2));
    }

    public void OnLeaveButton() {
        if (!Equals(state, BattleState.ReadyForLeave)) {
            return;
        }
        battleHUD.HideHUD();
        gameController.EndBattle(BattleState.Won);
    }

    #endregion

    #region Player actions

    IEnumerator PlayerAttack(int attacksCount) {
        int index = initHelper.GetCurrentUnitIndex();
        bool isDead = false;
        for (int i = 0; i < attacksCount; i++) {
            int damageRolled = Random.Range(heroes[index].DamageMin, heroes[index].DamageMax + 1);
            isDead = currentEnemy.TakeDamage(damageRolled);
            
            battleHUD.SetEnemyHP(currentEnemy.CurrentHealth);
            battleHUD.FlashEnemy();
            yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} hits {currentEnemy.UnitName} for {damageRolled} damage."));
            if (isDead) {
                state = BattleState.EnemyDefeated;
                yield return StartCoroutine(battleHUD.WriteLog($"{currentEnemy.UnitName} is dead."));
                battleHUD.EnemyDead();
                yield return new WaitForSeconds(3f);
                i = attacksCount;
                StartCoroutine(HandleNextEnemy());
            }
        }
        if (!isDead) {
            state = BattleState.PassingTurn;
            StartCoroutine(HandleTurn());
        }
    }
    IEnumerator PlayerUseAbility() {
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
        yield return null;
    }
    IEnumerator PlayerMeditate() {
        int index = initHelper.GetCurrentUnitIndex();
        heroes[index].GrantBuff(AbilityType.Meditate, _meditateLength);
        int prevEnergy = heroes[index].CurrentEnergy;
        int amountRolled = 2;
        heroes[index].Meditate(amountRolled);
        int energy = heroes[index].CurrentEnergy;

        battleHUD.SetHeroEnergy(index, heroes[index].CurrentEnergy);
        yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} meditates and restores {energy - prevEnergy} energy."));

        state = BattleState.PassingTurn;
        StartCoroutine(HandleTurn());
    }
    IEnumerator PlayerRecover() {
        int index = initHelper.GetCurrentUnitIndex();
        int prevHealth = heroes[index].CurrentHealth;
        int healRolled = (int)(Random.Range(20, 41) / 100f * heroes[index].MaxHealth);
        heroes[index].Heal(healRolled);
        int health = heroes[index].CurrentHealth;

        battleHUD.SetHeroHP(index, heroes[index].CurrentHealth);
        yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} bandages the wounds and gains {health - prevHealth} HP."));

        state = BattleState.PassingTurn;

        StartCoroutine(HandleTurn());
    }
    IEnumerator PlayerFlee() {
        int roll = Random.Range(0, 101);
        if (roll < _fleeChance) {
            state = BattleState.Fleed;
            yield return StartCoroutine(battleHUD.WriteLog("Fleed succeeded!"));
        } else {
            yield return StartCoroutine(battleHUD.WriteLog("Fleed unsuccessful!"));
            StartCoroutine(HandleTurn());
        }
    }
    IEnumerator PlayerProvoke() {
        int index = initHelper.GetCurrentUnitIndex();
        heroes[index].GrantBuff(AbilityType.Provoke, _provokeLength);
        yield return StartCoroutine(battleHUD.WriteLog(string.Format(heroes[index].Ability.Description, heroes[index].UnitName)));

        bool isDead = false;
        int attacksCount = 1;
        for (int i = 0; i < attacksCount; i++) {
            int damageRolled = Random.Range(heroes[index].DamageMin, heroes[index].DamageMax + 1);
            isDead = currentEnemy.TakeDamage(damageRolled);

            battleHUD.SetEnemyHP(currentEnemy.CurrentHealth);
            battleHUD.FlashEnemy();
            yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} hits {currentEnemy.UnitName} for {damageRolled} damage."));
            if (isDead) {
                state = BattleState.EnemyDefeated;
                yield return StartCoroutine(battleHUD.WriteLog($"{currentEnemy.UnitName} is dead."));
                i = attacksCount;
                StartCoroutine(HandleNextEnemy());
                break;
            }
        }
        if (!isDead) {
            state = BattleState.PassingTurn;
            StartCoroutine(HandleTurn());
        }
    }
    IEnumerator PlayerProtect() {
        int index = initHelper.GetCurrentUnitIndex();
        for (int i = 0; i < heroes.Count; i++) {
            heroes[i].GrantBuff(AbilityType.Protect, _protectLength);
        }
        yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} throw smoke grenade and hides all allies in it."));

        state = BattleState.PassingTurn;
        StartCoroutine(HandleTurn());
    }

    IEnumerator PlayerSearch() {
        var lootGenerator = new LootGenerator();
        item = lootGenerator.Generate(lair.lootRarity);
        yield return StartCoroutine(battleHUD.WriteLog($"You find {lair.goldReward} gold and {item.name}!"));
        yield return StartCoroutine(battleHUD.WriteLog($"To whom will you give {item.name}?"));
        var names = new List<string>();
        foreach (HeroUnit hero in heroes) {
            names.Add(hero.UnitName);
        }
        battleHUD.SetHeroChoices(names);
    }

    IEnumerator PlayerChooseHero(int index) {
        if (item.stat == LootStatUpgrade.Damage) {
            heroes[index].DamageMax += item.value;
            heroes[index].DamageMin += item.value;
        } else if (item.stat == LootStatUpgrade.Health) {
            heroes[index].MaxHealth += item.value;
        } else if (item.stat == LootStatUpgrade.Initiative) {
            heroes[index].InitiativeModifier += item.value;
        }
        battleHUD.SetHeroMaxHP(index, heroes[index].MaxHealth);
        yield return StartCoroutine(battleHUD.WriteLog($"{heroes[index].UnitName} takes {item.name}."));
        yield return StartCoroutine(battleHUD.WriteLog(lair.victoryEndText));
        battleHUD.SetLeaveHUD();
    }

    #endregion

    #endregion
}