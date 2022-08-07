using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour
{
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] List<HeroUnit> heroesPrefabs;
    [SerializeField] PlayerController playerController;
    List<HeroUnit> party = new List<HeroUnit>();
    [HideInInspector] public BattleState lastBattleResult = BattleState.None;
    [SerializeField] GameObject lostScreen;
    bool isGameLost = false;

    private void Start() {
        foreach (var hero in heroesPrefabs) {
            party.Add(Instantiate(hero));
        }
    }
    private void Update() {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
        if (Input.GetKey(KeyCode.R) && isGameLost) {
            // reload game;
        }
    }
    public void StartBattle(List<EnemyUnit> enemies, Lair lair) {
        playerController.isInputEnabled = false;
        StartCoroutine(battleSystem.StartNewBattle(party, enemies, lair));
    }
    public void EndBattle(BattleState state) {
        if (state == BattleState.Won) {
            lastBattleResult = state;
            foreach (var hero in party) {
                if (hero.CurrentHealth != 0) {
                    hero.Heal(hero.MaxHealth);
                }
            }
            playerController.isInputEnabled = true;
        } else if (state == BattleState.Lost) {
            lostScreen.SetActive(true);
            isGameLost = true;
        }
    }
}
