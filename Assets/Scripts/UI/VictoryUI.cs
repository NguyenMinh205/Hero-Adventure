using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VictoryUI : MonoBehaviour
{
    [Header("Popup Root")]
    [Tooltip("Transform của VictoryPopup (con trực tiếp, sẽ scale-in khi mở).")]
    [SerializeField] private Transform victoryPopup;
    [SerializeField] private Image dime;

    [Header("Animated Elements")]
    [Tooltip("Image chữ VICTORY ở đầu popup.")]
    [SerializeField] private CanvasGroup victoryImg;

    [Tooltip("Image rương kho báu.")]
    [SerializeField] private Transform chestRewardImg;

    [Header("Stars (chỉ dùng trong Level Mode)")]
    [Tooltip("Danh sách Star objects (3 cái). Ẩn hết ban đầu.")]
    [SerializeField] private List<GameObject> stars;

    [Header("Rewards")]
    [Tooltip("Prefab của RewardItem để khởi tạo động.")]
    [SerializeField] private RewardItem rewardPrefab;
    [Tooltip("Container chứa các RewardItem.")]
    [SerializeField] private Transform rewardContainer;
    
    [Header("Reward Sprites")]
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite expSprite;

    [Header("Buttons")]
    [Tooltip("Danh sách Button hiện sau khi xong rewards (theo thứ tự hiện dần).")]
    [SerializeField] private List<Button> buttons;

    [Header("Button Actions")]
    [Tooltip("Button Next Level.")]
    [SerializeField] private Button nextLevelButton;
    [Tooltip("Button Return to Menu.")]
    [SerializeField] private Button mainMenuButton;

    [Header("Timing (seconds)")]
    [SerializeField] private float popupScaleDuration = 0.45f;
    [SerializeField] private float victoryImgDuration = 0.5f;
    [SerializeField] private float chestImgDuration = 0.4f;
    [SerializeField] private float starStagger = 0.2f;
    [SerializeField] private float rewardStagger = 0.25f;
    [SerializeField] private float buttonStagger = 0.15f;
    [SerializeField] private float gapAfterTitle = 0.2f;
    [SerializeField] private float gapAfterChest = 0.3f;
    [SerializeField] private float gapAfterStars = 0.3f;
    [SerializeField] private float gapAfterRewards = 0.2f;

    [Header("Star HP Thresholds (%)")]
    [SerializeField] private float twoStarThreshold = 40f;
    [SerializeField] private float threeStarThreshold = 70f;

    [Header("Endless Base Rewards")]
    [SerializeField] private int baseGoldEndless = 50;
    [SerializeField] private int baseDiamondEndless = 1;
    [SerializeField] private int baseExpEndless = 30;

    private Coroutine _showCoroutine;

    private void Awake()
    {
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(OnNextLevelClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        if (nextLevelButton != null) nextLevelButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }

    public void Show(Player player = null, List<int> rewardOverrides = null)
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySoundWin();
        dime.gameObject.SetActive(true);
        PrepareInitialState();

        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(ShowSequence(player, rewardOverrides));
    }

    public void Hide()
    {
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        dime.gameObject.SetActive(false);
        if (victoryPopup != null) victoryPopup.gameObject.SetActive(false);
    }

    private void PrepareInitialState()
    {
        if (victoryPopup != null)
        {
            victoryPopup.gameObject.SetActive(true);
            victoryPopup.localScale = Vector3.zero;
        }

        if (victoryImg != null) { victoryImg.alpha = 0f; victoryImg.gameObject.SetActive(false); }

        if (chestRewardImg != null) { chestRewardImg.localScale = Vector3.zero; chestRewardImg.gameObject.SetActive(false); }

        if (stars != null)
            foreach (var s in stars)
                if (s != null) s.SetActive(false);

        if (rewardContainer != null)
        {
            foreach (Transform child in rewardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn == null) continue;
                CanvasGroup cg = GetOrAddCanvasGroup(btn.gameObject);
                cg.alpha = 0f;
                cg.interactable = false;
                cg.blocksRaycasts = false;
                btn.gameObject.SetActive(true);
            }
        }
    }

    private IEnumerator ShowSequence(Player player, List<int> rewardOverrides)
    {
        if (victoryPopup != null)
        {
            yield return victoryPopup
                .DOScale(1f, popupScaleDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle * 0.5f);

        if (victoryImg != null)
        {
            victoryImg.gameObject.SetActive(true);
            victoryImg.transform.localScale = Vector3.one * 1.3f;

            yield return DOTween.Sequence()
                .Append(victoryImg.DOFade(1f, victoryImgDuration * 0.5f))
                .Join(victoryImg.transform.DOScale(1f, victoryImgDuration).SetEase(Ease.OutElastic))
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle);

        if (chestRewardImg != null)
        {
            chestRewardImg.gameObject.SetActive(true);
            yield return chestRewardImg
                .DOScale(1f, chestImgDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterChest);

        bool isLevel = GameModeManager.Instance != null &&
                       GameModeManager.Instance.CurrentMode == GameModeType.Level;

        if (isLevel && stars != null && stars.Count > 0)
        {
            int starsToShow = CalculateStars(player);

            for (int i = 0; i < stars.Count; i++)
            {
                if (stars[i] == null) continue;

                if (i < starsToShow)
                {
                    stars[i].SetActive(true);
                    stars[i].transform.localScale = Vector3.zero;

                    yield return stars[i].transform
                        .DOScale(1f, 0.3f)
                        .SetEase(Ease.OutBack)
                        .SetUpdate(true)
                        .WaitForCompletion();

                    yield return new WaitForSecondsRealtime(starStagger);
                }
            }

            yield return new WaitForSecondsRealtime(gapAfterStars);
        }

        if (chestRewardImg != null) chestRewardImg.gameObject.SetActive(false);

        List<int> amounts = rewardOverrides ?? BuildRewardAmounts(isLevel);
        
        if (DataManager.Instance != null && amounts.Count >= 3)
        {
            DataManager.Instance.GameData.AddResources(amounts[0], amounts[1], amounts[2]);
            
            if (isLevel)
            {
                LevelConfig cfg = GameModeManager.Instance?.CurrentLevelConfig;
                if (cfg != null)
                {
                    DataManager.Instance.GameData.UnlockNextLevel(cfg.LevelID);
                }
            }
            
            DataManager.Instance.GameData.Save();
        }

        if (rewardPrefab != null && rewardContainer != null && amounts != null)
        {
            Sprite[] sprites = { goldSprite, diamondSprite, expSprite };

            for (int i = 0; i < amounts.Count; i++)
            {
                int amount = amounts[i];
                if (amount <= 0) continue;

                RewardItem newReward = Instantiate(rewardPrefab, rewardContainer);
                Sprite icon = (i < sprites.Length) ? sprites[i] : null;
                newReward.Init(icon, amount);
                newReward.PlayShowAnimation(0.35f);

                yield return new WaitForSecondsRealtime(rewardStagger);
            }
        }

        yield return new WaitForSecondsRealtime(gapAfterRewards);

        if (buttons != null)
        {
            foreach (var btn in buttons)
            {
                if (btn == null) continue;

                CanvasGroup cg = GetOrAddCanvasGroup(btn.gameObject);
                yield return cg.DOFade(1f, 0.25f)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        cg.interactable = true;
                        cg.blocksRaycasts = true;
                    })
                    .WaitForCompletion();

                yield return new WaitForSecondsRealtime(buttonStagger);
            }
        }
    }

    private int CalculateStars(Player player)
    {
        if (player == null) return 1;

        float hpPercent = (player.CurrentHealth / player.CurrentMaxHealth) * 100f;

        if (hpPercent >= threeStarThreshold) return 3;
        if (hpPercent >= twoStarThreshold) return 2;
        return 1;
    }

    private List<int> BuildRewardAmounts(bool isLevel)
    {
        if (isLevel)
        {
            LevelConfig cfg = GameModeManager.Instance?.CurrentLevelConfig;
            return new List<int>
            {
                cfg?.GoldReward    ?? 0,
                cfg?.DiamondReward ?? 0,
                cfg?.ExpReward     ?? 0
            };
        }
        else
        {
            int round = GetEndlessCurrentRound();
            float scale = 1f + (round - 1) * 0.25f;

            return new List<int>
            {
                Mathf.RoundToInt(baseGoldEndless    * scale),
                Mathf.RoundToInt(baseDiamondEndless * scale),
                Mathf.RoundToInt(baseExpEndless     * scale)
            };
        }
    }

    private int GetEndlessCurrentRound()
    {
        var strategy = BattleManager.Instance?.CurrentStrategy as EndlessModeStrategy;
        return strategy?.CurrentRound ?? 1;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private void OnNextLevelClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        
        bool isLevel = GameModeManager.Instance != null && GameModeManager.Instance.CurrentMode == GameModeType.Level;
        if (isLevel && GameModeManager.Instance.CurrentLevelConfig != null)
        {
            int nextLevelId = GameModeManager.Instance.CurrentLevelConfig.LevelID + 1;
            
            bool success = GameSceneManager.Instance != null &&
                           GameSceneManager.Instance.TryStartLevel(nextLevelId);
            if (!success)
            {
                GameSceneManager.Instance?.ShowMainMenu();
            }
        }
        else
        {
            BattleManager.Instance?.InitBattle();
        }
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        GameSceneManager.Instance?.ShowMainMenu();
    }
}
