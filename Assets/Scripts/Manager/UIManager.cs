using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public struct DamagePopupData
{
    public Vector3 Position;
    public int Damage;
    public bool IsCritical;
}

public class UIManager : MonoBehaviour
{
    [Header("Player Stats UI")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI shieldText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI critRateText;
    [SerializeField] private TextMeshProUGUI critDamageText;
    [SerializeField] private TextMeshProUGUI blockRateText;

    [Header("Turn & Round UI")]
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private GameObject[] turnIcons;

    [Header("Enemy Info Panel")]
    [SerializeField] private GameObject enemyInfoPanel;
    [SerializeField] private Image enemySprite;
    [SerializeField] private Image enemyHpFill; 
    [SerializeField] private TextMeshProUGUI enemyHpText;

    [Header("Damage Popups")]
    [SerializeField] private GameObject damagePopupObj;
    [SerializeField] private TextMeshProUGUI damagePopupText;

    [Header("Popups")]
    [SerializeField] private GameObject pausePopupObj;
    [SerializeField] private GameObject victoryPanelObj;
    [SerializeField] private GameObject gameOverPanelObj;

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowDamagePopup, HandleShowDamagePopup);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateTurnCount, UpdateTurnCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowEnemyInfo, ShowEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnHideEnemyInfo, HideEnemyInfo);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateEnemyHP, UpdateEnemyHP);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowDamagePopup, HandleShowDamagePopup);
    }

    private void Start()
    {
        enemyInfoPanel.SetActive(false);
        damagePopupObj.SetActive(false);
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

    private void HandleShowDamagePopup(object param)
    {
        if (param is DamagePopupData data)
        {
            Debug.Log($"Showing damage popup at {data.Position} with damage {data.Damage} (Critical: {data.IsCritical})");
            ShowDamagePopup(data.Position, data.Damage, data.IsCritical);
        }
    }

    public void ShowDamagePopup(Vector3 position, int damageAmount, bool isCritical)
    {
        damagePopupObj.transform.position = position;
        damagePopupObj.SetActive(true);

        damagePopupText.text = damageAmount.ToString();
        damagePopupText.color = isCritical ? Color.red : Color.yellow;
        damagePopupText.alpha = 1f;

        damagePopupText.transform.localScale = Vector3.one * (isCritical ? 1.5f : 1f);
        damagePopupObj.transform.DOKill();
        damagePopupText.DOKill();

        float endPosY = position.y + 1f;
        damagePopupObj.transform.DOMoveY(endPosY, 0.7f).SetEase(Ease.OutBack);

        damagePopupText.DOFade(0f, 0.7f).SetDelay(0.3f).OnComplete(() =>
        {
            damagePopupObj.SetActive(false);
        });
    }

    public void ShowPausePopup()
    {
        // Implement pause popup logic here (e.g., show a panel, display options, etc.)
    }

    public void ShowVictory()
    {
        // Implement victory UI logic here (e.g., show a panel, display rewards, etc.)
    }

    public void ShowGameOver()
    {
        // Implement game over UI logic here (e.g., show a panel, display final stats, etc.)
    }
}