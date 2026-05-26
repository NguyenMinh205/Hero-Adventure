using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            filledStar.gameObject.SetActive(isAchieved);
        }
    }
}
