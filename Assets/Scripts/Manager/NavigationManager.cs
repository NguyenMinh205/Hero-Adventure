using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public enum MenuTab
{
    Shop,
    Equipment,
    Battle,
    Talent,
    Reward
}

[System.Serializable]
public class NavigationTabItem
{
    public MenuTab tabType;
    public Button tabButton;
    public GameObject tabPanel;
    public Color highlightColor = Color.white;
}

public class NavigationManager : MonoBehaviour
{
    [Header("Tabs Configuration")]
    [SerializeField] private NavigationTabItem[] tabs;

    [Header("Highlight UI")]
    [SerializeField] private RectTransform highlightRect;
    [SerializeField] private float highlightMoveDuration = 0.2f;
    [SerializeField] private Ease highlightEase = Ease.OutQuad;

    [Header("Coming Soon Popup")]
    [SerializeField] private GameObject inProgressPopupObj;
    [SerializeField] private RectTransform inProgresspopupPanel;
    [SerializeField] private Button popupOkBtn;

    private NavigationTabItem currentTabItem;

    public void InitializeNavigation()
    {
        StartCoroutine(InitRoutine());
    }

    private IEnumerator InitRoutine()
    {
        if (popupOkBtn != null) popupOkBtn.onClick.AddListener(HideComingSoonPopup);
        if (inProgressPopupObj != null) inProgressPopupObj.SetActive(false);
        if (inProgresspopupPanel != null) inProgresspopupPanel.localScale = Vector3.zero;

        foreach (var tab in tabs)
        {
            if (tab.tabButton != null)
            {
                MenuTab type = tab.tabType;
                tab.tabButton.onClick.AddListener(() => SwitchTab(type));
            }
        }
        
        yield return new WaitForEndOfFrame();

        SwitchTab(MenuTab.Battle, false);
    }

    private void OnDestroy()
    {
        foreach (var tab in tabs)
        {
            if (tab.tabButton != null)
            {
                tab.tabButton.onClick.RemoveAllListeners();
            }
        }

        if (popupOkBtn != null) popupOkBtn.onClick.RemoveAllListeners();
    }

    public void SwitchTab(MenuTab targetTab, bool animateHighlight = true)
    {
        if (animateHighlight)
        {
            AudioManager.Instance?.PlaySoundClick();
        }

        if (currentTabItem != null && currentTabItem.tabPanel != null)
        {
            currentTabItem.tabPanel.SetActive(false);
        }

        NavigationTabItem newTabItem = null;
        foreach (var tab in tabs)
        {
            if (tab.tabType == targetTab)
            {
                newTabItem = tab;
                break;
            }
        }

        if (newTabItem != null)
        {
            currentTabItem = newTabItem;

            if (currentTabItem.tabPanel != null)
            {
                currentTabItem.tabPanel.SetActive(true);
            }

            if (highlightRect != null && currentTabItem.tabButton != null)
            {
                RectTransform btnRect = currentTabItem.tabButton.GetComponent<RectTransform>();
                Image highlightImg = highlightRect.GetComponent<Image>();
                
                if (animateHighlight)
                {
                    highlightRect.DOMoveX(btnRect.position.x, highlightMoveDuration).SetEase(highlightEase);
                    if (highlightImg != null)
                    {
                        highlightImg.DOColor(currentTabItem.highlightColor, highlightMoveDuration);
                    }
                }
                else
                {
                    Vector3 newPos = highlightRect.position;
                    newPos.x = btnRect.position.x;
                    highlightRect.position = newPos;
                    if (highlightImg != null)
                    {
                        highlightImg.color = currentTabItem.highlightColor;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning($"[Lỗi UI] Không tìm thấy cấu hình cho Tab: {targetTab} trong mảng Tabs của NavigationManager!");
        }

        if (animateHighlight && targetTab != MenuTab.Battle)
        {
            ShowComingSoonPopup();
        }
    }

    private void ShowComingSoonPopup()
    {
        if (inProgressPopupObj == null || inProgresspopupPanel == null) return;
        
        AudioManager.Instance?.PlayPopupOpen();
        inProgressPopupObj.SetActive(true);
        inProgresspopupPanel.localScale = Vector3.zero;
        inProgresspopupPanel.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void HideComingSoonPopup()
    {
        if (inProgressPopupObj == null || inProgresspopupPanel == null) return;
        
        AudioManager.Instance?.PlaySoundClick();
        AudioManager.Instance?.PlayPopupClose();

        inProgresspopupPanel.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
        {
            inProgressPopupObj.SetActive(false);
        });
    }
}
