using UnityEngine;

public class PickableObject : MonoBehaviour
{
    [Header("Object Properties")]
    [SerializeField] private string objectName = "Vật phẩm";
    [SerializeField] private float weight = 1f; // Trọng lượng
    [SerializeField] private bool isFragile = false; // Dễ vỡ
    
    [Header("Break Settings")]
    [SerializeField] private float breakVelocity = 15f; // Tốc độ va chạm để vỡ
    [SerializeField] private GameObject brokenPrefab; // Model vỡ (optional)
    
    private Rigidbody rb;
    private bool isBroken = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.mass = weight;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Kiểm tra va chạm mạnh nếu là đồ dễ vỡ
        if (isFragile && !isBroken && rb != null)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            
            if (impactForce > breakVelocity)
            {
                Break();
            }
        }
    }

    public bool CanPickup()
    {
        return !isBroken;
    }

    public void OnPickup()
    {
        // Có thể thêm hiệu ứng âm thanh, particle
        Debug.Log("Đã nhặt: " + objectName);
    }

    public void OnDrop()
    {
        // Có thể thêm hiệu ứng âm thanh
        Debug.Log("Đã thả: " + objectName);
    }

    public void OnThrow()
    {
        // Có thể thêm hiệu ứng âm thanh ném
        Debug.Log("Đã ném: " + objectName);
    }

    void Break()
    {
        isBroken = true;
        Debug.Log(objectName + " đã vỡ!");
        
        // Spawn đồ vật vỡ nếu có
        if (brokenPrefab != null)
        {
            Instantiate(brokenPrefab, transform.position, transform.rotation);
        }
        
        // Có thể thêm particle, âm thanh vỡ ở đây
        
        // Xóa object gốc
        Destroy(gameObject, 0.1f);
    }

    public float GetWeight()
    {
        return weight;
    }

    public string GetObjectName()
    {
        return objectName;
    }
}