using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class UIManager : MonoBehaviour
{
    [Header("Player Stats UI")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI shieldText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI critRateText;
    public TextMeshProUGUI critDamageText;
    public TextMeshProUGUI blockRateText;

    [Header("Turn & Round UI")]
    public TextMeshProUGUI roundText;
    public GameObject[] turnIcons;

    [Header("Enemy Info Panel")]
    public GameObject enemyInfoPanel;
    public Image enemySprite;
    public Image enemyHpFill; 
    public TextMeshProUGUI enemyHpText;

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
    }

    private void Start()
    {
        enemyInfoPanel.SetActive(false);
    }

    private void UpdatePlayerStats(object param)
    {
        if (param is Player player)
        {
            hpText.text = $"{player.CurrentHealth}/{player.CurrentMaxHealth}";
            shieldText.text = $"{player.CurrentShield}";
            damageText.text = $"{player.CurrentDamage}";
            critRateText.text = $"{player.CurrentCritRate}%";
            critDamageText.text = $"{player.CurrentCritDamage}%";
            blockRateText.text = $"{player.CurrentBlockRate}%";
        }
    }

    private void UpdateTurnCount(object param)
    {
        int currentTurns = (int)param;
        for (int i = 0; i < turnIcons.Length; i++)
        {
            turnIcons[i].SetActive(i < currentTurns);
        }
    }

    private void UpdateRoundCount(object param)
    {
        int round = (int)param;
        roundText.text = round.ToString();
    }

    private void ShowEnemyInfo(object param)
    {
        if (param is Enemy enemy)
        {
            enemyInfoPanel.SetActive(true);
            enemySprite.sprite = enemy.CharacterSprite;
            UpdateEnemyHP(enemy);
        }
    }

    private void HideEnemyInfo(object param)
    {
        enemyInfoPanel.SetActive(false);
    }

    private void UpdateEnemyHP(object param)
    {
        if (param is Enemy enemy)
        {
            enemyHpText.text = $"{enemy.CurrentHealth}/{enemy.CurrentMaxHealth}";
            float fillAmount = enemy.CurrentHealth / enemy.CurrentMaxHealth;

            enemyHpFill.DOFillAmount(fillAmount, 0.3f).SetEase(Ease.OutQuad);
        }
    }
}