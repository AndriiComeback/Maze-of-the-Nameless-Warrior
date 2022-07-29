using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] List<TMP_Text> heroesNames;
    [SerializeField] List<TMP_Text> heroesHealthes;
    [SerializeField] List<TMP_Text> heroesEnergies;
    [SerializeField] TMP_Text enemyInfo;
    [SerializeField] TMP_Text battleLog;
    [SerializeField] int maxBattleLogLines = 7;
    [SerializeField] Button abilityButton;
    [SerializeField] Image enemyImage;

    public void SetHUD(List<HeroUnit> heroes, Unit enemy, bool clearLog = true) {
        for (int i = 0; i < heroesNames.Count && i < heroes.Count; i++) {
            heroesNames[i].text = $"{heroes[i].UnitName}";
            heroesHealthes[i].text = $"{heroes[i].CurrentHealth}/{heroes[i].MaxHealth}";
            heroesEnergies[i].text = $"{heroes[i].CurrentEnergy}/{heroes[i].MaxEnergy}";
        }

        enemyInfo.text = $"{enemy.UnitName} ({enemy.CurrentHealth}/{enemy.MaxHealth} HP)";
        if (clearLog) {
            Clear();
        }
    }
    public void WriteLog(string text) {
        text = $"> {text}\n\n";
        if (battleLog.text.Split('\n').Length >= maxBattleLogLines) {
            var lines = battleLog.text.Split('\n').Skip(1).ToArray();
            battleLog.text = string.Join('\n', lines);
        }
        if (!string.IsNullOrWhiteSpace(battleLog.text)) {
            battleLog.text += '\n';
        }
        battleLog.text += text;
    }
    public void Clear() {
        battleLog.text = string.Empty;
    }
    public void HighlightPlayer(int heroOrderNumber) {
        heroesNames[heroOrderNumber].text = $">>> {heroesNames[heroOrderNumber].text}";
    }
    public void ClearHighlight() {
        for (int i = 0; i < heroesNames.Count; i++) {
            heroesNames[i].text = heroesNames[i].text.Replace(">>> ", "");
        }
    }
    public void SetEnemyHP(int health) {
        enemyInfo.text = Regex.Replace(enemyInfo.text, "\\([0-9]+\\/", $"({health}/");
    }
    public void SetHeroHP(int index, int health) {
        heroesHealthes[index].text = Regex.Replace(heroesHealthes[index].text, "[0-9]+\\/", $"{health}/");
    }
    public void SetHeroEnergy(int index, int energy) {
        heroesEnergies[index].text = Regex.Replace(heroesEnergies[index].text, "[0-9]+\\/", $"{energy}/");
    }
    public void SetAbilityButtonText(string text) {
        abilityButton.GetComponentInChildren<TMP_Text>().text = text;
    }
    public void SetAbilityButtonTextActive(bool active) {
        abilityButton.interactable = active;
    }
    public void SetEnemyImage(Sprite image) {
        enemyImage.gameObject.SetActive(true);
        enemyImage.sprite = image;
    }
}
