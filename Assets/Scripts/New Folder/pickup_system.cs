using UnityEngine;

public class MovingOutStylePickup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform holdPosition; // Vị trí cầm đồ
    [SerializeField] private Camera playerCamera;
    
    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private float maxCarryWeight = 50f; // Trọng lượng tối đa
    [SerializeField] private LayerMask pickupLayer;
    
    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpForce = 2f;
    
    [Header("Input")]
    [SerializeField] private KeyCode pickupKey = KeyCode.E;
    [SerializeField] private KeyCode throwKey = KeyCode.Mouse0; // Click chuột trái để ném
    
    private PickableObject currentObject;
    private bool isCarrying = false;
    private float originalSpeed;
    
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
            
        // Tạo hold position nếu chưa có
        if (holdPosition == null)
        {
            GameObject holdPoint = new GameObject("HoldPosition");
            holdPoint.transform.SetParent(playerCamera.transform);
            holdPoint.transform.localPosition = new Vector3(0, -0.5f, 1.5f);
            holdPosition = holdPoint.transform;
        }
        
        // Lưu tốc độ gốc (nếu có character controller)
        var controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            // Bạn cần tự lưu speed từ script movement của mình
        }
    }

    void Update()
    {
        // Hiển thị gợi ý khi nhìn vào đồ vật
        CheckForPickupPrompt();
        
        if (Input.GetKeyDown(pickupKey))
        {
            if (!isCarrying)
            {
                TryPickup();
            }
            else
            {
                Drop();
            }
        }
        
        // Ném đồ vật
        if (Input.GetKeyDown(throwKey) && isCarrying)
        {
            ThrowObject();
        }
    }

    void CheckForPickupPrompt()
    {
        if (isCarrying) return;
        
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            PickableObject pickable = hit.collider.GetComponent<PickableObject>();
            if (pickable != null)
            {
                // Hiển thị UI prompt ở đây (ví dụ: "Nhấn E để nhặt")
                Debug.Log("Có thể nhặt: " + hit.collider.name);
            }
        }
    }

    void TryPickup()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            PickableObject pickable = hit.collider.GetComponent<PickableObject>();
            
            if (pickable != null && pickable.CanPickup())
            {
                // Kiểm tra trọng lượng
                Rigidbody rb = pickable.GetComponent<Rigidbody>();
                if (rb != null && rb.mass > maxCarryWeight)
                {
                    Debug.Log("Đồ vật quá nặng!");
                    return;
                }
                
                PickupObject(pickable);
            }
        }
    }

    void PickupObject(PickableObject pickable)
    {
        currentObject = pickable;
        Rigidbody rb = currentObject.GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            // Tắt physics khi đang cầm
            rb.isKinematic = true;
            rb.interpolation = RigidbodyInterpolation.None;
        }
        
        // Gắn vào vị trí cầm
        currentObject.transform.SetParent(holdPosition);
        currentObject.transform.localPosition = Vector3.zero;
        currentObject.transform.localRotation = Quaternion.identity;
        
        // Tắt collider để không va chạm với player
        Collider col = currentObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        currentObject.OnPickup();
        isCarrying = true;
        
        // Giảm tốc độ di chuyển
        ApplyCarrySpeedModifier();
        
        Debug.Log("Đã nhặt: " + currentObject.name);
    }

    void Drop()
    {
        if (currentObject == null) return;
        
        Rigidbody rb = currentObject.GetComponent<Rigidbody>();
        
        // Bật lại physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Tách khỏi player
        currentObject.transform.SetParent(null);
        
        // Bật lại collider
        Collider col = currentObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        currentObject.OnDrop();
        currentObject = null;
        isCarrying = false;
        
        // Khôi phục tốc độ
        RestoreOriginalSpeed();
        
        Debug.Log("Đã thả đồ vật");
    }

    void ThrowObject()
    {
        if (currentObject == null) return;
        
        Rigidbody rb = currentObject.GetComponent<Rigidbody>();
        
        // Bật lại physics
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        // Tách khỏi player
        currentObject.transform.SetParent(null);
        
        // Bật lại collider
        Collider col = currentObject.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = true;
        }
        
        // Ném theo hướng nhìn
        if (rb != null)
        {
            Vector3 throwDirection = playerCamera.transform.forward;
            Vector3 throwVelocity = throwDirection * throwForce + Vector3.up * throwUpForce;
            rb.velocity = throwVelocity;
            
            // Thêm xoay ngẫu nhiên
            rb.angularVelocity = Random.insideUnitSphere * 2f;
        }
        
        currentObject.OnThrow();
        currentObject = null;
        isCarrying = false;
        
        // Khôi phục tốc độ
        RestoreOriginalSpeed();
        
        Debug.Log("Đã ném đồ vật");
    }

    void ApplyCarrySpeedModifier()
    {
        // Áp dụng giảm tốc độ cho script di chuyển của bạn
        // Ví dụ nếu dùng CharacterController hoặc custom movement script
    }

    void RestoreOriginalSpeed()
    {
        // Khôi phục tốc độ gốc
    }

    public bool IsCarrying()
    {
        return isCarrying;
    }

    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 direction = playerCamera.transform.forward;
            Gizmos.DrawRay(playerCamera.transform.position, direction * pickupRange);
        }
        
        if (holdPosition != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(holdPosition.position, 0.2f);
        }
    }
}