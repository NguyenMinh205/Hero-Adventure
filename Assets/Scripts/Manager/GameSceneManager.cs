using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : MonoBehaviour
{
    [Header("Top Bar UI")]
    [SerializeField] private TextMeshProUGUI topBarLevelText;
    [SerializeField] private Image topBarExpFill;
    [SerializeField] private TextMeshProUGUI topBarExpText;
    [SerializeField] private TextMeshProUGUI topBarGoldText;
    [SerializeField] private TextMeshProUGUI topBarDiamondText;
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject selectLevelPanel;
    [SerializeField] private GameObject gameplayPanel;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button adventureButton;
    [SerializeField] private Button endlessButton;

    [Header("Select Level Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button[] levelNodeButtons;
    [SerializeField] private LevelConfig[] levelConfigs; 
    
    [Header("Level Node Assets")]
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private Sprite unlockedLevelSprite;
    
    public void Start()
    {
        ShowMainMenu();

        if (adventureButton != null)
            adventureButton.onClick.AddListener(OnAdventureClicked);
            
        if (endlessButton != null)
            endlessButton.onClick.AddListener(OnEndlessClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);

        for (int i = 0; i < levelNodeButtons.Length; i++)
        {
            int levelIndex = i;
            if (levelNodeButtons[i] != null)
            {
                levelNodeButtons[i].onClick.AddListener(() => OnLevelNodeClicked(levelIndex));
            }
        }
    }

    private void OnDestroy()
    {
        if (adventureButton != null) adventureButton.onClick.RemoveAllListeners();
        if (endlessButton != null) endlessButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
        foreach (var btn in levelNodeButtons)
        {
            if (btn != null) btn.onClick.RemoveAllListeners();
        }
    }

    public void ShowMainMenu()
    {
        UpdateTopBar();
        if (mainMenuPanel) mainMenuPanel.SetActive(true);
        if (selectLevelPanel) selectLevelPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(false);
    }

    private void UpdateTopBar()
    {
        if (DataManager.Instance == null) return;

        var data = DataManager.Instance.GameData;
        
        if (topBarLevelText != null) topBarLevelText.text = $"{data.PlayerLevel}";
        
        int maxExp = data.PlayerLevel * 100;
        if (topBarExpText != null) topBarExpText.text = $"{data.CurrentExp}/{maxExp}";
        if (topBarExpFill != null) topBarExpFill.fillAmount = (float)data.CurrentExp / maxExp;

        if (topBarGoldText != null) topBarGoldText.text = data.Gold.ToString();
        if (topBarDiamondText != null) topBarDiamondText.text = data.Diamond.ToString();
    }

    private void OnAdventureClicked()
    {
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (selectLevelPanel) selectLevelPanel.SetActive(true);
        if (gameplayPanel) gameplayPanel.SetActive(false);
        
        UpdateLevelNodes();
    }

    private void UpdateLevelNodes()
    {
        if (DataManager.Instance == null) return;

        int maxUnlocked = DataManager.Instance.GameData.MaxUnlockedLevel;

        for (int i = 0; i < levelNodeButtons.Length; i++)
        {
            if (levelNodeButtons[i] != null)
            {
                bool isUnlocked = (i <= maxUnlocked);
                levelNodeButtons[i].interactable = isUnlocked;

                // Đổi Sprite giữa khoá và mở khoá
                Image btnImage = levelNodeButtons[i].GetComponent<Image>();
                if (btnImage != null && lockedLevelSprite != null && unlockedLevelSprite != null)
                {
                    btnImage.sprite = isUnlocked ? unlockedLevelSprite : lockedLevelSprite;
                }

                // Ẩn/hiện số level
                TextMeshProUGUI levelText = levelNodeButtons[i].GetComponentInChildren<TextMeshProUGUI>(true);
                if (levelText != null)
                {
                    levelText.gameObject.SetActive(isUnlocked);
                }
            }
        }
    }

    private void OnEndlessClicked()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetGameMode(GameModeType.Endless);
        }
        
        if (mainMenuPanel) mainMenuPanel.SetActive(false);
        if (selectLevelPanel) selectLevelPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(true);

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.InitBattle();
        }
    }

    private void OnBackClicked()
    {
        ShowMainMenu();
    }

    private void OnLevelNodeClicked(int levelIndex)
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.SetGameMode(GameModeType.Level);
            
            if (levelConfigs != null && levelIndex >= 0 && levelIndex < levelConfigs.Length)
            {
                GameModeManager.Instance.SetLevelConfig(levelConfigs[levelIndex]);
            }
            else
            {
                Debug.LogWarning($"Không tìm thấy LevelConfig cho level index: {levelIndex}");
            }
        }

        if (selectLevelPanel) selectLevelPanel.SetActive(false);
        if (gameplayPanel) gameplayPanel.SetActive(true);

        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.InitBattle();
        }
    }
}
