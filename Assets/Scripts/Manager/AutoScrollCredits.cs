using UnityEngine;
using UnityEngine.UI;

public class AutoScrollCredits : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float scrollSpeed = 30f;

    private float contentHeight;
    private float viewportHeight;
    private float scrollPosition;

    void OnEnable()
    {
        scrollRect.verticalNormalizedPosition = 0f;
        scrollPosition = 0f;

        contentHeight = scrollRect.content.rect.height;
        viewportHeight = scrollRect.viewport.rect.height;
    }

    void Update()
    {
        if (contentHeight <= viewportHeight) return;

        scrollPosition += scrollSpeed * Time.deltaTime;
        float maxScroll = contentHeight - viewportHeight;

        float normalized = Mathf.Clamp01(scrollPosition / maxScroll);
        scrollRect.verticalNormalizedPosition = 1f - normalized;
    }
}
