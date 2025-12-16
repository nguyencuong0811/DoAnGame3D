// Assets/Scripts/Level/LevelConfig.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Min(1)] public int timeLimitSeconds = 120;
    public string nextSceneName = ""; // để trống nếu chưa có màn sau

    [Serializable]
    public class ItemRequirement
    {
        public string itemId;      // ví dụ: "TV", "Sofa", "BoxSmall"
        [Min(1)] public int count = 1;
    }

    public List<ItemRequirement> requirements = new();
}
