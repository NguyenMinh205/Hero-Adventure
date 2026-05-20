using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DefeatUI : MonoBehaviour
{
    [Header("Popup Root")]
    [Tooltip("Transform của DefeatPopup (con trực tiếp của panel, sẽ scale-in khi mở).")]
    [SerializeField] private Transform defeatPopup;
    [SerializeField] private Image dime;

    [Header("Animated Elements")]
    [Tooltip("Image chữ DEFEAT ở đầu popup.")]
    [SerializeField] private CanvasGroup defeatImg;

    [Tooltip("Image thanh gươm gãy.")]
    [SerializeField] private Transform swordBrokenImg;

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
    [Tooltip("Button Retry — reload lại trậy.")]
    [SerializeField] private Button retryButton;
    [Tooltip("Button Return to Menu.")]
    [SerializeField] private Button mainMenuButton;

    [Header("Timing (seconds)")]
    [SerializeField] private float popupScaleDuration = 0.45f;
    [SerializeField] private float defeatImgDuration = 0.5f;
    [SerializeField] private float swordImgDuration = 0.4f;
    [SerializeField] private float rewardStagger = 0.25f;
    [SerializeField] private float buttonStagger = 0.15f;
    [SerializeField] private float gapAfterTitle = 0.2f;
    [SerializeField] private float gapAfterSword = 0.3f;
    [SerializeField] private float gapAfterRewards = 0.2f;

    [Header("Endless Base Rewards")]
    [SerializeField] private int baseGoldEndless = 30;
    [SerializeField] private int baseDiamondEndless = 0;
    [SerializeField] private int baseExpEndless = 20;

    private Coroutine _showCoroutine;


    private void Awake()
    {
        if (retryButton != null) retryButton.onClick.AddListener(OnRetryClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnDestroy()
    {
        if (retryButton != null) retryButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }

    public void Show(List<int> rewardOverrides = null)
    {
        dime.gameObject.SetActive(true);
        PrepareInitialState();

        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        _showCoroutine = StartCoroutine(ShowSequence(rewardOverrides));
    }

    public void Hide()
    {
        if (_showCoroutine != null) StopCoroutine(_showCoroutine);
        dime.gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────────────────
    //  PRIVATE
    // ──────────────────────────────────────────────────────────────────────────

    private void PrepareInitialState()
    {
        // Popup bắt đầu scale = 0
        if (defeatPopup != null)
            defeatPopup.localScale = Vector3.zero;

        // DefeatImg ẩn
        if (defeatImg != null) { defeatImg.alpha = 0f; defeatImg.gameObject.SetActive(false); }

        // SwordBroken ẩn
        if (swordBrokenImg != null) { swordBrokenImg.localScale = Vector3.zero; swordBrokenImg.gameObject.SetActive(false); }

        // Clear old rewards
        if (rewardContainer != null)
        {
            foreach (Transform child in rewardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Reset buttons
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

    private IEnumerator ShowSequence(List<int> rewardOverrides)
    {
        // ── 1. Popup scale-in ─────────────────────────────────────────────────
        if (defeatPopup != null)
        {
            yield return defeatPopup
                .DOScale(1f, popupScaleDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle * 0.5f);

        // ── 2. DefeatImg fade + bounce ────────────────────────────────────────
        if (defeatImg != null)
        {
            defeatImg.gameObject.SetActive(true);
            defeatImg.transform.localScale = Vector3.one * 1.3f;

            yield return DOTween.Sequence()
                .Append(defeatImg.DOFade(1f, defeatImgDuration * 0.5f))
                .Join(defeatImg.transform.DOScale(1f, defeatImgDuration).SetEase(Ease.OutElastic))
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterTitle);

        // ── 3. SwordBrokenImg scale-in ────────────────────────────────────────
        if (swordBrokenImg != null)
        {
            swordBrokenImg.gameObject.SetActive(true);
            yield return swordBrokenImg
                .DOScale(1f, swordImgDuration)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(gapAfterSword);

        // ── 4. Rewards lần lượt ───────────────────────────────────────────────
        List<int> amounts = rewardOverrides ?? BuildRewardAmounts();
        
        // Lưu data
        if (DataManager.Instance != null && amounts.Count >= 3)
        {
            DataManager.Instance.GameData.AddResources(amounts[0], amounts[1], amounts[2]);
            DataManager.Instance.GameData.Save();
        }

        if (rewardPrefab != null && rewardContainer != null && amounts != null)
        {
            Sprite[] sprites = { goldSprite, diamondSprite, expSprite };

            for (int i = 0; i < amounts.Count; i++)
            {
                int amount = amounts[i];
                if (amount <= 0) continue; // Chỉ hiện phần thưởng có số lượng > 0

                RewardItem newReward = Instantiate(rewardPrefab, rewardContainer);
                Sprite icon = (i < sprites.Length) ? sprites[i] : null;
                newReward.Init(icon, amount);
                newReward.PlayShowAnimation(0.35f);

                yield return new WaitForSecondsRealtime(rewardStagger);
            }
        }

        yield return new WaitForSecondsRealtime(gapAfterRewards);

        // ── 5. Buttons lần lượt ───────────────────────────────────────────────
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

    /// <summary>
    /// Tính danh sách phần thưởng dựa vào mode hiện tại.
    /// Thứ tự mặc định: [Gold, Diamond, Exp] (khớp với rewards list trong Inspector).
    /// </summary>
    private List<int> BuildRewardAmounts()
    {
        bool isLevel = GameModeManager.Instance != null &&
                       GameModeManager.Instance.CurrentMode == GameModeType.Level;

        if (isLevel)
        {
            // Lấy từ LevelConfig
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
            // Endless: base x scalingFactor theo round
            int round = GetEndlessCurrentRound();

            // round 1 → x1.00, round 2 → x1.25, round 3 → x1.50, ...
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
        // EndlessModeStrategy cần expose CurrentRound — xem EndlessModeStrategy.cs
        var strategy = BattleManager.Instance?.CurrentStrategy as EndlessModeStrategy;
        return strategy?.CurrentRound ?? 1;
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject go)
    {
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    private void OnRetryClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        BattleManager.Instance?.InitBattle();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        Hide();
        FindObjectOfType<GameSceneManager>()?.ShowMainMenu();
    }
}
