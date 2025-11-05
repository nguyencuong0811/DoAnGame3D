// Assets/Scripts/Level/LevelGoalManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class LevelGoalManager : MonoBehaviour
{
    [Header("Config")]
    public LevelConfig config;

    [Header("Optional UI")]
    public TextMeshProUGUI timerText;             // để trống nếu chưa có UI
    public TextMeshProUGUI objectiveText;         // để trống nếu chưa có UI
    public GameObject winPanel;        // bật lên khi thắng
    public GameObject losePanel;       // bật lên khi thua

    private float _timeLeft;
    private Dictionary<string, int> _required; // itemId -> count
    private CarryableItem[] _allItems;
    private bool _ended;

    void Start()
    {
        if (config == null)
        {
            Debug.LogError("LevelGoalManager: Chưa gán LevelConfig!");
            enabled = false;
            return;
        }

        _timeLeft = config.timeLimitSeconds;
        _required = config.requirements
            .GroupBy(r => r.itemId)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.count));

        _allItems = FindObjectsOfType<CarryableItem>(includeInactive: false);

        UpdateObjectiveUI();
        UpdateTimerUI();
        ToggleResultPanels(false, false);
    }

    void Update()
    {
        if (_ended) return;

        // đếm ngược thời gian
        _timeLeft -= Time.deltaTime;
        if (_timeLeft < 0f) _timeLeft = 0f;
        UpdateTimerUI();

        // kiểm tra mục tiêu
        if (AllRequirementsSatisfied())
        {
            EndLevel(true);
            return;
        }

        // hết giờ mà chưa đủ
        if (_timeLeft <= 0f)
        {
            EndLevel(false);
        }
    }

    private bool AllRequirementsSatisfied()
    {
        // đếm số item đang Loaded theo itemId
        var counts = new Dictionary<string, int>();
        foreach (var item in _allItems)
        {
            if (item == null) continue;
            if (!item.IsLoaded) continue;

            if (!counts.ContainsKey(item.itemId)) counts[item.itemId] = 0;
            counts[item.itemId]++;
        }

        // so với yêu cầu
        foreach (var kv in _required)
        {
            counts.TryGetValue(kv.Key, out int have);
            if (have < kv.Value) return false;
        }
        return true;
    }

    private void EndLevel(bool win)
    {
        _ended = true;
        ToggleResultPanels(win, !win);

        if (win)
        {
            Debug.Log("LEVEL CLEAR!");
            // Auto chuyển màn nếu có cấu hình
            if (!string.IsNullOrEmpty(config.nextSceneName))
            {
                // chờ 1–2 giây cho người chơi xem UI rồi chuyển (tùy thích)
                Invoke(nameof(LoadNextScene), 1.5f);
            }
        }
        else
        {
            Debug.Log("TIME UP! LEVEL FAILED!");
        }
    }

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(config.nextSceneName))
        {
            SceneManager.LoadScene(config.nextSceneName);
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int sec = Mathf.CeilToInt(_timeLeft);
        int m = sec / 60;
        int s = sec % 60;
        timerText.text = $"{m:00}:{s:00}";
    }

    private void UpdateObjectiveUI()
    {
        if (objectiveText == null) return;

        // tạo bảng “Cần: x / y” theo từng itemId
        var counts = new Dictionary<string, int>();
        foreach (var item in _allItems)
        {
            if (item == null) continue;
            if (!item.IsLoaded) continue;
            counts[item.itemId] = counts.GetValueOrDefault(item.itemId) + 1;
        }

        System.Text.StringBuilder sb = new();
        sb.AppendLine("MỤC TIÊU:");
        foreach (var req in _required)
        {
            counts.TryGetValue(req.Key, out int have);
            sb.AppendLine($"- {req.Key}: {have}/{req.Value}");
        }
        objectiveText.text = sb.ToString();
    }

    private void ToggleResultPanels(bool win, bool lose)
    {
        if (winPanel != null) winPanel.SetActive(win);
        if (losePanel != null) losePanel.SetActive(lose);
    }

    // Gọi hàm này nếu muốn refresh UI theo chu kỳ (ví dụ mỗi 0.2s) để đỡ tốn Update()
    public void RefreshObjectiveUI() => UpdateObjectiveUI();

    // Bạn có thể kéo thả Event này vào LoadZone/CarryableItem khi trạng thái loaded đổi để update UI ngay
    void LateUpdate()
    {
        if (_ended) return;
        // cập nhật UI mục tiêu mỗi vài frame cho nhẹ
        if (Time.frameCount % 10 == 0) UpdateObjectiveUI();
    }
}
