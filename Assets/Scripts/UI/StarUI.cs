using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StarUI : MonoBehaviour
{
    [SerializeField] private Image filledStar;
    [SerializeField] private TextMeshProUGUI requireTxt;

    public void Setup(string conditionText, bool isAchieved)
    {
        if (requireTxt != null)
        {
            requireTxt.text = conditionText;
        }

        if (filledStar != null)
        {
            if (isAchieved)
            {
                filledStar.gameObject.SetActive(true);
                filledStar.transform.localScale = Vector3.zero;
                filledStar.transform.DOKill();
                filledStar.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack).SetDelay(0.2f).SetUpdate(true);
            }
            else
            {
                filledStar.gameObject.SetActive(false);
            }
        }
    }
}
