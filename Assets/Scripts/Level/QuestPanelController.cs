using UnityEngine;

public class QuestPanelController : MonoBehaviour
{
    public RectTransform questPanel;    // gán QuestPanel vào đây
    public float slideDuration = 0.5f;  // thời gian slide
    public float hiddenX = -350f;       // vị trí panel khi ẩn
    public float visibleX = 50f;        // vị trí panel khi hiện ra
    public KeyCode toggleKey = KeyCode.M; // phím để mở/đóng panel

    private bool isVisible = false;
    private float slideSpeed;

    void Start()
    {
        questPanel.anchoredPosition = new Vector2(hiddenX, questPanel.anchoredPosition.y);
        slideSpeed = Mathf.Abs(visibleX - hiddenX) / slideDuration;
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            isVisible = !isVisible;
        }

        if (isVisible && questPanel.anchoredPosition.x < visibleX)
        {
            float newX = questPanel.anchoredPosition.x + slideSpeed * Time.deltaTime;
            if (newX > visibleX) newX = visibleX;
            questPanel.anchoredPosition = new Vector2(newX, questPanel.anchoredPosition.y);
        }
        else if (!isVisible && questPanel.anchoredPosition.x > hiddenX)
        {
            float newX = questPanel.anchoredPosition.x - slideSpeed * Time.deltaTime;
            if (newX < hiddenX) newX = hiddenX;
            questPanel.anchoredPosition = new Vector2(newX, questPanel.anchoredPosition.y);
        }
    }
}
