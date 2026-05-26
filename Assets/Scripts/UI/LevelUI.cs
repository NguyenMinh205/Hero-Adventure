using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUI : MonoBehaviour
{
    [SerializeField] private Button levelButton;
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private List<Image> listStars;
    [SerializeField] private Sprite filledStarSprite;

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

        if (listStars != null && listStars.Count > 0)
        {
            int earnedStars = DataManager.Instance != null ? DataManager.Instance.GameData.GetLevelStar(levelIndex) : 0;
            for (int i = 0; i < listStars.Count; i++)
            {
                if (listStars[i] == null) continue;
                
                if (isUnlocked && i < earnedStars && filledStarSprite != null)
                {
                    listStars[i].sprite = filledStarSprite;
                }
            }
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
