using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI levelText;

    private int levelIndex;

    public void Setup(int index, bool isUnlocked, Sprite lockedSprite, Sprite unlockedSprite)
    {
        levelIndex = index;

        if (levelButton != null)
        {
            levelButton.interactable = isUnlocked;
            levelButton.onClick.RemoveAllListeners();
            levelButton.onClick.AddListener(OnClicked);
        }

        if (bgImage != null && lockedSprite != null && unlockedSprite != null)
        {
            bgImage.sprite = isUnlocked ? unlockedSprite : lockedSprite;
        }

        if (levelText != null)
        {
            levelText.gameObject.SetActive(isUnlocked);
        }
    }

    private void OnClicked()
    {
        GameSceneManager.Instance.OnLevelNodeClicked(levelIndex);
    }

    private void OnDestroy()
    {
        if (levelButton != null)
        {
            levelButton.onClick.RemoveAllListeners();
        }
    }
}
