using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class IntroSceneManager : MonoBehaviour
{
    [Header("Intro Loading")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject characterIntro;
    [SerializeField] private Image loadingBar;
    [SerializeField] private TextMeshProUGUI loadingText;

    [Header("Intro Button")]
    [SerializeField] private GameObject introButtonPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private float sequentialDelay = 0.2f;

    private AsyncOperation asyncLoadOperation;

    private void Start()
    {
        introButtonPanel.SetActive(false);
        loadingPanel.SetActive(true);
        characterIntro.SetActive(true);
        loadingBar.fillAmount = 0f;
        loadingText.text = "Loading 0%";

        playButton.transform.localScale = Vector3.zero;
        settingButton.transform.localScale = Vector3.zero;
        quitButton.transform.localScale = Vector3.zero;

        playButton.onClick.AddListener(OnPlayClicked);
        quitButton.onClick.AddListener(OnQuitClicked);

        DOVirtual.DelayedCall(1f, () =>
        {
            StartCoroutine(LoadSceneAsync());
        });
    }

    private IEnumerator LoadSceneAsync()
    {
        asyncLoadOperation = SceneManager.LoadSceneAsync(1);

        asyncLoadOperation.allowSceneActivation = false;

        while (!asyncLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoadOperation.progress / 0.9f);

            loadingBar.DOFillAmount(progress, 0.2f);
            loadingText.text = $"Loading {(progress * 100):0}%";

            if (asyncLoadOperation.progress >= 0.9f)
            {
                yield return new WaitForSeconds(2f);
                ShowButtonsPanel();
                break;
            }

            yield return null;
        }
    }

    private void ShowButtonsPanel()
    {
        loadingPanel.SetActive(false);
        characterIntro.SetActive(false);
        introButtonPanel.SetActive(true);

        playButton.transform.DOScale(1f, popupDuration).SetEase(Ease.OutBack);

        settingButton.transform.DOScale(1f, popupDuration)
            .SetEase(Ease.OutBack)
            .SetDelay(sequentialDelay);

        quitButton.transform.DOScale(1f, popupDuration)
            .SetEase(Ease.OutBack)
            .SetDelay(sequentialDelay * 2f);
    }

    private void OnPlayClicked()
    {
        DOTween.KillAll();

        if (asyncLoadOperation != null)
        {
            asyncLoadOperation.allowSceneActivation = true;
        }
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked. Exiting application...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
