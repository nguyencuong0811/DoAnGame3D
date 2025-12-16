// Assets/Scripts/Level/CarryableItem.cs
using UnityEngine;

[DisallowMultipleComponent]
public class CarryableItem : MonoBehaviour
{
    [Tooltip("Phải trùng với itemId trong LevelConfig")]
    public string itemId = "BoxSmall";

    [Tooltip("Nếu bạn có hệ thống cầm nắm, gọi SetBeingCarried(true/false) khi nhặt/thả.")]
    public bool beingCarried = false;

    public bool IsLoaded { get; private set; } = false;

    public void SetBeingCarried(bool value) => beingCarried = value;

    // Được LoadZone gọi khi nằm trong vùng xe và không bị cầm
    public void SetLoaded(bool value)
    {
        if (IsLoaded == value) return;
        IsLoaded = value;
        // Có thể play VFX/SFX ở đây
        // Debug.Log($"{name} loaded = {IsLoaded}");
    }
}
