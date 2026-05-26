using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSceneManager : Singleton<GameSceneManager>
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
    [SerializeField] private LevelUI[] levelNodeUIs;
    [SerializeField] private LevelConfig[] levelConfigs; 
    
    [Header("Level Node Assets")]
    [SerializeField] private Sprite lockedLevelSprite;
    [SerializeField] private Sprite unlockedLevelSprite;
    [SerializeField] private Sprite lockedBossLevelSprite;
    [SerializeField] private Sprite unlockedBossLevelSprite;
    
    public void Start()
    {
        ShowMainMenu();
        AudioManager.Instance?.PlayMusicInMenu();

        if (adventureButton != null)
            adventureButton.onClick.AddListener(OnAdventureClicked);
            
        if (endlessButton != null)
            endlessButton.onClick.AddListener(OnEndlessClicked);

        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
    }

    private void OnDestroy()
    {
        if (adventureButton != null) adventureButton.onClick.RemoveAllListeners();
        if (endlessButton != null) endlessButton.onClick.RemoveAllListeners();
        if (backButton != null) backButton.onClick.RemoveAllListeners();
    }

    public void ShowMainMenu()
    {
        BattleManager.Instance?.CleanupBattle();
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

        for (int i = 0; i < levelNodeUIs.Length; i++)
        {
            if (levelNodeUIs[i] != null)
            {
                bool isUnlocked = (i <= maxUnlocked);
                bool isBossLevel = false;

                if (levelConfigs != null && i < levelConfigs.Length && levelConfigs[i] != null)
                {
                    isBossLevel = levelConfigs[i].IsBossLevel;
                }

                Sprite lockedSprite = isBossLevel && lockedBossLevelSprite != null ? lockedBossLevelSprite : lockedLevelSprite;
                Sprite unlockedSprite = isBossLevel && unlockedBossLevelSprite != null ? unlockedBossLevelSprite : unlockedLevelSprite;

                levelNodeUIs[i].Setup(i, isUnlocked, lockedSprite, unlockedSprite);
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

    public void OnLevelNodeClicked(int levelIndex)
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

    /// <summary>
    /// Thử bắt đầu level theo LevelID. Trả về false nếu không tìm thấy level.
    /// </summary>
    public bool TryStartLevel(int levelId)
    {
        if (levelConfigs == null) return false;

        for (int i = 0; i < levelConfigs.Length; i++)
        {
            if (levelConfigs[i] != null && levelConfigs[i].LevelID == levelId)
            {
                OnLevelNodeClicked(i);
                return true;
            }
        }

        return false;
    }
}

