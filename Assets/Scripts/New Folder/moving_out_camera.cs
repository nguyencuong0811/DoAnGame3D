using UnityEngine;

public class MovingOutCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // Player
    
    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0, 10, -8);
    [SerializeField] private float distance = 12f;
    [SerializeField] private float height = 8f;
    [SerializeField] private float angle = 45f; // Góc nhìn xuống
    
    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private bool lookAtTarget = true;
    
    [Header("Zoom Settings")]
    [SerializeField] private bool allowZoom = true;
    [SerializeField] private float minZoom = 8f;
    [SerializeField] private float maxZoom = 15f;
    [SerializeField] private float zoomSpeed = 2f;
    
    [Header("Rotation Settings")]
    [SerializeField] private bool allowRotation = true;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private KeyCode rotateLeftKey = KeyCode.Q;
    [SerializeField] private KeyCode rotateRightKey = KeyCode.E;
    
    private float currentRotation = 0f;
    private float currentZoom;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        currentZoom = distance;
        
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Set vị trí ban đầu
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null) return;
        
        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        if (!allowRotation) return;
        
        // Xoay camera bằng Q/E
        if (Input.GetKey(rotateLeftKey))
        {
            currentRotation += rotationSpeed * Time.deltaTime;
        }
        if (Input.GetKey(rotateRightKey))
        {
            currentRotation -= rotationSpeed * Time.deltaTime;
        }
    }

    void HandleZoom()
    {
        if (!allowZoom) return;
        
        // Zoom bằng scroll chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }

    void UpdateCameraPosition()
    {
        // Tính vị trí camera dựa trên góc và khoảng cách
        Quaternion rotation = Quaternion.Euler(angle, currentRotation, 0);
        Vector3 position = target.position - (rotation * Vector3.forward * currentZoom);
        position.y = target.position.y + height;
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, position, smoothSpeed * Time.deltaTime);
        
        // Nhìn vào player
        if (lookAtTarget)
        {
            Vector3 lookPosition = target.position;
            lookPosition.y += 1f; // Nhìn vào giữa người thay vì chân
            transform.LookAt(lookPosition);
        }
    }

    // Hàm để set camera ở góc nhất định (cho cutscene, v.v.)
    public void SetCameraAngle(float newAngle)
    {
        currentRotation = newAngle;
    }

    // Hàm để shake camera (khi va chạm mạnh, v.v.)
    public void ShakeCamera(float intensity, float duration)
    {
        StartCoroutine(CameraShake(intensity, duration));
    }

    private System.Collections.IEnumerator CameraShake(float intensity, float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            
            transform.position = new Vector3(
                originalPosition.x + x,
                originalPosition.y + y,
                originalPosition.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    void OnDrawGizmosSelected()
    {
        if (target == null) return;
        
        // Vẽ line từ camera đến target
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, target.position);
        
        // Vẽ sphere ở target
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, 0.5f);
    }
}