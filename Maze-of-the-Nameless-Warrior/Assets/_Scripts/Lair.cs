using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lair : MonoBehaviour
{
    [SerializeField] public string enemyCountDescription;
    [SerializeField] List<EnemyUnit> enemyUnitsPrefabs;
    [SerializeField] GameController gameController;
    [SerializeField] public LootRarity lootRarity;
    [SerializeField] public int goldReward;
    [SerializeField] public string victoryEndText;
    [SerializeField] GameObject defeatedLiarSprite;
    [SerializeField] public Sprite liarImage;
    [SerializeField] public string liarName;
    List<EnemyUnit> enemyParty = new List<EnemyUnit>();
    private void Start() {
        foreach (var enemy in enemyUnitsPrefabs) {
            enemyParty.Add(Instantiate(enemy));
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            gameController.StartBattle(enemyParty, this);
        }
    }
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            if (gameController.lastBattleResult == BattleState.Won) {
                Instantiate(defeatedLiarSprite, gameObject.transform);
                Destroy(gameObject);
            }
        }
    }
}
