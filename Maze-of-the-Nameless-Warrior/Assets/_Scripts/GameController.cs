using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GameController : MonoBehaviour
{
    [SerializeField] BattleSystem battleSystem;
    [SerializeField] List<HeroUnit> heroes;
    [SerializeField] List<EnemyUnit> enemies;
    private void Start() {
        //"<color=#850700>Goblin</color> yells at you."

        StartCoroutine(battleSystem.StartNewBattle(heroes,enemies));
    }
    private void Update() {
        if (Input.GetKey("escape")) {
            Application.Quit();
        }
    }
}
