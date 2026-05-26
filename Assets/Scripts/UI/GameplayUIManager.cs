using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using DG.Tweening;

public struct DamagePopupData
{
    public Vector3 Position;
    public int Damage;
    public bool IsCritical;
}

public class GameplayUIManager : MonoBehaviour
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

    [Header("Pause Popup")]
    [SerializeField] private PausePopup pausePopup;
    [SerializeField] private Button pauseButton;

    [Header("Victory Panel")]
    [SerializeField] private VictoryUI victoryUI;

    [Header("Game Over Panel")]
    [SerializeField] private DefeatUI defeatUI;

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnShowDamagePopup, HandleShowDamagePopup);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdatePlayerStats, UpdatePlayerStats);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnUpdateRoundCount, UpdateRoundCount);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnShowDamagePopup, HandleShowDamagePopup);
    }

    public void Init()
    {
        if (enemyInfoPanel != null) enemyInfoPanel.SetActive(false);
        if (damagePopupObj != null) damagePopupObj.SetActive(false);
        if (victoryUI != null) victoryUI.Hide();
        if (defeatUI != null) defeatUI.Hide();

        if (pausePopup != null) pausePopup.Init();
        
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OnPauseClicked);
            pauseButton.onClick.AddListener(OnPauseClicked);
        }
    }

    private void OnDestroy()
    {
        if (pauseButton != null) pauseButton.onClick.RemoveAllListeners();
    }

    private void OnPauseClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        ObserverManager<EventID>.PostEvent(EventID.OnPause);
    }

    public void ShowVictory(Player player = null)
    {
        
        if (victoryUI != null)
            victoryUI.Show(player);
    }

    private Coroutine _gameOverCoroutine;

    public void ShowGameOver(Player player = null)
    {
        
        if (_gameOverCoroutine != null) StopCoroutine(_gameOverCoroutine);
        _gameOverCoroutine = StartCoroutine(ShowGameOverDelayed());
    }

    private IEnumerator ShowGameOverDelayed()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        if (defeatUI != null) defeatUI.Show();
        _gameOverCoroutine = null;
    }

    public void UpdateTurnCount(int currentTurns)
    {
        if (turnIcons == null) return;
        for (int i = 0; i < turnIcons.Length; i++)
        {
            if (turnIcons[i] != null) turnIcons[i].SetActive(i < currentTurns);
        }
    }

    public void ShowEnemyInfo(Enemy enemy)
    {
        if (enemy == null) return;
        if (enemyInfoPanel != null) enemyInfoPanel.SetActive(true);
        if (enemySprite != null) enemySprite.sprite = enemy.CharacterSprite;
        RefreshEnemyHP(enemy);
    }

    public void HideEnemyInfo()
    {
        if (enemyInfoPanel != null) enemyInfoPanel.SetActive(false);
    }

    private void UpdatePlayerStats(object param)
    {
        if (param is Player player)
        {
            if (hpText != null) hpText.text = $"{player.CurrentHealth}/{player.CurrentMaxHealth}";
            if (shieldText != null) shieldText.text = $"{player.CurrentShield}";
            if (damageText != null) damageText.text = $"{player.CurrentDamage}";
            if (critRateText != null) critRateText.text = $"{player.CurrentCritRate}%";
            if (critDamageText != null) critDamageText.text = $"{player.CurrentCritDamage}%";
            if (blockRateText != null) blockRateText.text = $"{player.CurrentBlockRate}%";
        }
    }

    private void UpdateRoundCount(object param)
    {
        int round = (int)param;
        if (roundText != null) roundText.text = round.ToString();
    }

    public void RefreshEnemyHP(Enemy enemy)
    {
        if (enemyHpText != null)
            enemyHpText.text = $"{enemy.CurrentHealth}/{enemy.CurrentMaxHealth}";
        if (enemyHpFill != null)
        {
            float fillAmount = enemy.CurrentHealth / enemy.CurrentMaxHealth;
            enemyHpFill.DOFillAmount(fillAmount, 0.3f).SetEase(Ease.OutQuad);
        }
    }

    private void HandleShowDamagePopup(object param)
    {
        if (param is DamagePopupData data)
            ShowDamagePopup(data.Position, data.Damage, data.IsCritical);
    }

    public void ShowDamagePopup(Vector3 position, int damageAmount, bool isCritical)
    {
        if (damagePopupObj == null || damagePopupText == null) return;

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
            if (damagePopupObj != null) damagePopupObj.SetActive(false);
        });
    }
}