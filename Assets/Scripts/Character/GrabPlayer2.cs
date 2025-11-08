using UnityEngine;

public class ObjectGrabbing : MonoBehaviour
{
    [Header("Grabbing Settings")]
    public float grabRadius = 1.5f; // Bán kính "vùng" nhặt đồ xung quanh player
    public KeyCode grabKey = KeyCode.E; // Nút để nhặt/thả

    // ĐIỂM CẦM NÀY PHẢI LÀ CON CỦA PLAYER
    public Transform holdPoint;

    // Biến nội bộ
    private GameObject heldObject = null;
    private Rigidbody heldObjectRb = null;

    void Update()
    {
        if (Input.GetKeyDown(grabKey))
        {
            // --- 1. Nếu CHƯA cầm gì -> Thử nhặt ---
            if (heldObject == null)
            {
                // Tìm vật thể "Grabbable" gần nhất
                GameObject closestObject = FindClosestGrabbableObject();

                // Nếu tìm thấy, nhặt nó
                if (closestObject != null)
                {
                    GrabObject(closestObject);
                }
            }
            // --- 2. Nếu ĐANG cầm gì -> Thả ra ---
            else
            {
                DropObject();
            }
        }
    }

    private GameObject FindClosestGrabbableObject()
    {
        // 1. Tạo một "vùng" hình cầu xung quanh vị trí của player
        // transform.position là vị trí của player
        Collider[] colliders = Physics.OverlapSphere(transform.position, grabRadius);

        GameObject closest = null;
        float minDistance = float.MaxValue;

        // 2. Duyệt qua tất cả các vật thể trong vùng đó
        foreach (Collider col in colliders)
        {
            // 3. Nếu vật thể có tag "Grabbable"
            if (col.CompareTag("Grabbable"))
            {
                // 4. Tính khoảng cách và tìm cái gần nhất
                float distance = Vector3.Distance(transform.position, col.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = col.gameObject;
                }
            }
        }

        return closest;
    }

    // --- CÁC HÀM NÀY GIỮ NGUYÊN SO VỚI TRƯỚC ---

    void GrabObject(GameObject obj)
    {
        heldObject = obj;

        // Lấy Rigidbody của vật thể
        if (obj.TryGetComponent<Rigidbody>(out heldObjectRb))
        {
            // Tắt trọng lực và vật lý động (để nó bay theo tay)
            heldObjectRb.isKinematic = true;
        }

        // Gắn vật thể vào "HoldPoint" (là con của player)
        obj.transform.parent = holdPoint;

        // Di chuyển vật thể về chính giữa HoldPoint
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity; // Reset xoay
    }

    void DropObject()
    {
        // Thả vật thể ra (không còn là "con" của HoldPoint)
        heldObject.transform.parent = null;

        // Bật lại trọng lực và vật lý (để nó rơi)
        if (heldObjectRb != null)
        {
            heldObjectRb.isKinematic = false;
        }

        // Reset biến
        heldObject = null;
        heldObjectRb = null;
    }

    // (Tùy chọn) Vẽ ra vùng nhặt đồ trong Scene View để dễ debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, grabRadius);
    }
}