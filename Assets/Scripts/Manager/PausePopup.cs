using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PausePopup : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image backgroundDim;
    [SerializeField] private GameObject popupContentObj;
    [SerializeField] private Transform popupContentTransform;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;

    public void Init()
    {
        if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

        if (popupContentObj != null) popupContentObj.SetActive(false);
    }

    private void OnEnable()
    {
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnPause, HandlePause);
        ObserverManager<EventID>.AddRegisterEvent(EventID.OnResume, HandleResume);
    }

    private void OnDisable()
    {
        ObserverManager<EventID>.RemoveAddListener(EventID.OnPause, HandlePause);
        ObserverManager<EventID>.RemoveAddListener(EventID.OnResume, HandleResume);
    }

    private void OnDestroy()
    {
        if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
        if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
        if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
    }

    private void HandlePause(object param)
    {
        Show();
    }

    private void HandleResume(object param)
    {
        Hide();
    }

    public void Show()
    {
        if (popupContentObj == null || popupContentTransform == null) return;

        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 0f;
        backgroundDim.gameObject.SetActive(true);
        popupContentObj.SetActive(true);
        popupContentTransform.localScale = Vector3.zero;
        popupContentTransform
            .DOScale(1f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void Hide()
    {
        if (popupContentTransform == null) return;

        AudioManager.Instance?.PlaySoundButtonClick();
        popupContentTransform
            .DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (popupContentObj != null) popupContentObj.SetActive(false);
                if (backgroundDim != null) backgroundDim.gameObject.SetActive(false);
                Time.timeScale = 1f;
            });
    }

    private void OnResumeClicked()
    {
        ObserverManager<EventID>.PostEvent(EventID.OnResume);
    }

    private void OnSettingsClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        SettingsManager.Instance?.OpenSettings();
    }

    private void OnMainMenuClicked()
    {
        AudioManager.Instance?.PlaySoundButtonClick();
        Time.timeScale = 1f;
        if (popupContentObj != null) popupContentObj.SetActive(false);
        FindObjectOfType<GameSceneManager>()?.ShowMainMenu();
    }
}
