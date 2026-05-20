using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RewardItem : MonoBehaviour
{
    [SerializeField] private Image iconImg;
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private CanvasGroup cg;

    public void Init(Sprite icon, int amount)
    {
        if (iconImg != null && icon != null)
            iconImg.sprite = icon;

        if (amountText != null)
            amountText.text = amount.ToString();
    }
    public void PlayShowAnimation(float duration = 0.35f)
    {
        gameObject.SetActive(true);
        cg.alpha = 0f;
        transform.localScale = Vector3.zero;

        DOTween.Sequence()
            .Append(transform.DOScale(1f, duration).SetEase(Ease.OutBack))
            .Join(cg.DOFade(1f, duration * 0.8f))
            .SetUpdate(true);
    }

    public void Reset()
    {
        gameObject.SetActive(false);
        transform.localScale = Vector3.zero;
        cg.alpha = 0f;
    }
}
