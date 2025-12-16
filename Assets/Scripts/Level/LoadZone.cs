// Assets/Scripts/Level/LoadZone.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LoadZone : MonoBehaviour
{
    private readonly HashSet<CarryableItem> _inside = new();

    void OnTriggerEnter(Collider other)
    {
        var item = other.GetComponentInParent<CarryableItem>();
        if (item != null) _inside.Add(item);
    }

    void OnTriggerExit(Collider other)
    {
        var item = other.GetComponentInParent<CarryableItem>();
        if (item != null)
        {
            _inside.Remove(item);
            item.SetLoaded(false); // rời khỏi xe → không còn được tính
        }
    }

    void FixedUpdate()
    {
        foreach (var item in _inside)
        {
            if (item == null) continue;
            // Chỉ đánh dấu loaded khi KHÔNG bị cầm
            bool canBeCounted = !item.beingCarried;
            item.SetLoaded(canBeCounted);
        }
    }
}
