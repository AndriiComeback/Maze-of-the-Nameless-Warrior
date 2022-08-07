using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class BattleHUD : MonoBehaviour
{
    [SerializeField] GameObject battleMenu;
    [SerializeField] List<TMP_Text> heroesNames;
    [SerializeField] List<TMP_Text> heroesHealthes;
    [SerializeField] List<TMP_Text> heroesEnergies;
    [SerializeField] TMP_Text enemyInfo;
    [SerializeField] TMP_Text battleLog;
    [SerializeField] int maxBattleLogLines = 7;
    [SerializeField, Range(0, 100)]  int _printSpeed = 5;
    [SerializeField] List<GameObject> highLights;
    [SerializeField] Image enemyImage;
    [SerializeField] GameObject actionsPanel;
    [SerializeField] GameObject searchPanel;
    [SerializeField] GameObject chooseHeroPanel;
    [SerializeField] GameObject leavePanel;
    [SerializeField] GameObject backGround1;
    [SerializeField] GameObject backGround2;
    [SerializeField] GameObject enemyBackGround1;
    [SerializeField] GameObject enemyBackGround2;

    [Header("Buttons")]
    [SerializeField] Button attackButton;
    [SerializeField] Button abilityButton;
    [SerializeField] Button meditateButton;
    [SerializeField] Button recoverButton;
    [SerializeField] Button fleeButton;
    [SerializeField] Button firstHero;
    [SerializeField] Button secondHero;
    [SerializeField] Button thirdHero;


    private List<string> queue = new List<string>();
    private bool isTyping = false;

    #region Methods: public

    public void SetHUD(List<HeroUnit> heroes, Unit enemy, bool clearLog = true) {
        battleMenu.SetActive(true);
        actionsPanel.SetActive(true);
        searchPanel.SetActive(false);
        chooseHeroPanel.SetActive(false);
        leavePanel.SetActive(false);
        for (int i = 0; i < heroesNames.Count && i < heroes.Count; i++) {
            heroesNames[i].text = $"{heroes[i].UnitName}";
            heroesHealthes[i].text = $"{heroes[i].CurrentHealth}/{heroes[i].MaxHealth}";
            heroesEnergies[i].text = $"{heroes[i].CurrentEnergy}/{heroes[i].MaxEnergy}";
        }
        enemyInfo.gameObject.SetActive(true);
        enemyInfo.text = $"{enemy.UnitName} ({enemy.CurrentHealth} HP)";
        if (clearLog) {
            Clear();
        }
        attackButton.interactable = false;
        abilityButton.interactable = false;
        meditateButton.interactable = false;
        recoverButton.interactable = false;
        fleeButton.interactable = false;
    }
    public void HideHUD() {
        battleMenu.SetActive(false);
    }

    public void SwitchToSearch(Sprite lairImage) {
        actionsPanel.SetActive(false);
        searchPanel.SetActive(true);
        SetEnemyImage(lairImage, true);
    }
    public void SetHeroChoices(List<string> names) {
        chooseHeroPanel.SetActive(true);
        searchPanel.SetActive(false);
        if (names.Count == 3) {
            firstHero.GetComponentInChildren<TMP_Text>().text = names[0];
            secondHero.GetComponentInChildren<TMP_Text>().text = names[1];
            thirdHero.GetComponentInChildren<TMP_Text>().text = names[2];
        }
    }
    public void SetLeaveHUD() {
        chooseHeroPanel.SetActive(false);
        leavePanel.SetActive(true);
    }
    public IEnumerator WriteLog(string text) {
        if (!isTyping) {
            FixLines();
            yield return StartCoroutine(PrintByLetter(text));
        } else {
            queue.Add(text);
        }
        yield return null;
    }
    public void Clear() {
        battleLog.text = string.Empty;
    }
    public void HighlightPlayer(int heroOrderNumber) {
        highLights[heroOrderNumber].SetActive(true);
        attackButton.interactable = true;
        meditateButton.interactable = true;
        recoverButton.interactable = true;
        fleeButton.interactable = true;
    }
    public void ClearHighlight() {
        for (int i = 0; i < heroesNames.Count; i++) {
            highLights[i].SetActive(false);
        }
        attackButton.interactable = false;
        abilityButton.interactable = false;
        meditateButton.interactable = false;
        recoverButton.interactable = false;
        fleeButton.interactable = false;
    }
    public void FlashHUD() {
        StartCoroutine(Blink(backGround1, backGround2));
    }
    public void FlashEnemy() {
        StartCoroutine(Blink(enemyBackGround1, enemyBackGround2));
    }
    public void EnemyDead() {
        StartCoroutine(EnemyImageDissapear());
    }
    IEnumerator EnemyImageDissapear() {
        for (int i = 100; i > 1; i--) {
            enemyImage.color = new Color(enemyImage.color.r, enemyImage.color.g, enemyImage.color.b, i / 100f);
            yield return new WaitForSeconds(0.015f);
        }
        yield return new WaitForSeconds(1.5f);
    }
    public void ShowEnemy() {
        enemyImage.color = new Color(enemyImage.color.r, enemyImage.color.g, enemyImage.color.b, 1);
    }
    IEnumerator Blink(GameObject b1, GameObject b2) {
        int i = 0;
        bool t = false;
        while (i < 15) {
            if (t) {
                yield return new WaitForSeconds(0.03f);
                b1.SetActive(true);
                b2.SetActive(false);
                t = false;
            } else {
                yield return new WaitForSeconds(0.03f);
                b1.SetActive(false);
                b2.SetActive(true);
                t = true;
            }
            i++;
        }
        b1.SetActive(true);
        b2.SetActive(false);
    }
    public void SetEnemyHP(int health) {
        enemyInfo.text = Regex.Replace(enemyInfo.text, "\\([0-9]+", $"({health}");
    }
    public void SetHeroHP(int index, int health) {
        heroesHealthes[index].text = Regex.Replace(heroesHealthes[index].text, "[0-9]+\\/", $"{health}/");
    }
    public void SetHeroMaxHP(int index, int maxHealth) {
        heroesHealthes[index].text = Regex.Replace(heroesHealthes[index].text, "\\/[0-9]+", $"/{maxHealth}");
    }
    public void SetHeroEnergy(int index, int energy) {
        heroesEnergies[index].text = Regex.Replace(heroesEnergies[index].text, "[0-9]+\\/", $"{energy}/");
    }
    public void SetAbilityButton(string text, bool isEnabled) {
        abilityButton.GetComponentInChildren<TMP_Text>().text = text;
        abilityButton.interactable = isEnabled;
    }
    public void SetEnemyImage(Sprite image, bool hideHP = false) {
        enemyImage.gameObject.SetActive(true);
        enemyImage.color = new Color(enemyImage.color.r, enemyImage.color.g, enemyImage.color.b, 1);
        enemyImage.sprite = image;
        if (hideHP) {
            enemyInfo.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Methods: private

    private IEnumerator PrintByLetter(string text) {
        isTyping = true;

        foreach (char c in text) {
            battleLog.text += c;
            yield return new WaitForSeconds(0.1f/ _printSpeed);
        }
        battleLog.text += "\n\n";
        isTyping = false;
        if (queue != null && queue.Count > 0) {
            var txt = queue[0];
            queue.RemoveAt(0);
            FixLines();
            yield return StartCoroutine(PrintByLetter(txt));
        }
    }
    private void FixLines() {
        if (battleLog.text.Split('\n').Length >= maxBattleLogLines) {
            var lines = battleLog.text.Split('\n').Skip(1).ToArray();
            battleLog.text = string.Join('\n', lines);
        }
        if (!string.IsNullOrWhiteSpace(battleLog.text)) {
            battleLog.text += '\n';
        }
    }

    #endregion
}
