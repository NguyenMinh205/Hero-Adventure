using System.Collections;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform bgRectTransform;

    [Header("Settings")]
    [SerializeField] private float scrollSpeed = 200f;

    [SerializeField] private float tileWidth = 1000f;

    private Coroutine scrollCoroutine;
    private Vector2 startPosition;

    private void Start()
    {
        if (bgRectTransform != null)
        {
            startPosition = bgRectTransform.anchoredPosition;
        }
    }

    public void StartScrolling()
    {
        if (scrollCoroutine == null)
        {
            scrollCoroutine = StartCoroutine(ScrollRoutine());
        }
    }

    public void StopScrolling()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    private IEnumerator ScrollRoutine()
    {
        while (true)
        {
            if (bgRectTransform != null)
            {
                bgRectTransform.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

                if (bgRectTransform.anchoredPosition.x <= startPosition.x - tileWidth)
                {
                    float overshoot = (startPosition.x - tileWidth) - bgRectTransform.anchoredPosition.x;
                    bgRectTransform.anchoredPosition = new Vector2(startPosition.x - overshoot, startPosition.y);
                }
            }
            yield return null;
        }
    }
}