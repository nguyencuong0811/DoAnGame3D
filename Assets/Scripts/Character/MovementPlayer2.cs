using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementPlayer2 : MonoBehaviour
{
    // === CÁC BIẾN CÀI ĐẶT ===
    [Header("Movement Settings")]
    public float moveSpeed = 5f; // Tốc độ di chuyển
    public float jumpForce = 7f; // Lực nhảy
    
    // MỚI: Biến tốc độ xoay
    [Header("Rotation Settings")]
    public float rotationSpeed = 15f; // Tốc độ nhân vật xoay mặt

    [Header("Ground Check Settings")]
    public Transform groundCheck; // Vị trí để kiểm tra "chạm đất"
    public float groundDistance = 0.4f; // Bán kính của quả cầu vô hình để check đất
    public LayerMask groundMask; // Layer (lớp) nào được coi là "mặt đất"

    // === BIẾN NỘI BỘ ===
    private Rigidbody rb;
    private bool isGrounded;
    private bool shouldJump;
    
    // MỚI: Biến để lưu hướng di chuyển và tham chiếu đến camera
    private Vector3 _moveDirection;
    private Transform _cameraMainTransform;

    // Start được gọi 1 lần khi game bắt đầu
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Vẫn khóa xoay tự động do va chạm
        
        // MỚI: Lấy transform của camera chính
        // Đảm bảo camera của bạn được tag "MainCamera" trong Inspector
        _cameraMainTransform = Camera.main.transform;
    }

    // Update được gọi mỗi frame (dùng để xử lý Input)
    void Update()
    {
        // 1. KIỂM TRA CHẠM ĐẤT (Giữ nguyên)
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // 2. LẤY INPUT DI CHUYỂN (Giữ nguyên)
        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            verticalInput = 1f;
        else if (Input.GetKey(KeyCode.DownArrow))
            verticalInput = -1f;

        if (Input.GetKey(KeyCode.RightArrow))
            horizontalInput = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow))
            horizontalInput = -1f;

        // 3. TÍNH TOÁN HƯỚNG DI CHUYỂN (THAY ĐỔI LỚN)
        // Lấy hướng "trước mặt" và "bên phải" của camera
        Vector3 camForward = _cameraMainTransform.forward;
        Vector3 camRight = _cameraMainTransform.right;

        // Làm phẳng vector (không di chuyển lên/xuống theo hướng camera nhìn)
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Tính toán hướng di chuyển cuối cùng dựa trên input và hướng camera
        _moveDirection = (camForward * verticalInput + camRight * horizontalInput).normalized;

        // 4. LẤY INPUT NHẢY (Giữ nguyên)
        if ((Input.GetKeyDown(KeyCode.Space)) && isGrounded)
        {
            shouldJump = true; 
        }
    }

    // FixedUpdate được gọi cố định số lần mỗi giây (dùng để xử lý Vật lý)
    void FixedUpdate()
    {
        // 1. ÁP DỤNG DI CHUYỂN VÀ XOAY (THAY ĐỔI LỚN)
        if (_moveDirection.magnitude >= 0.1f)
        {
            // --- Xoay nhân vật ---
            // Tính toán hướng mà nhân vật cần xoay tới
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            
            // Sử dụng Slerp để xoay nhân vật mượt mà
            // Chúng ta dùng rb.rotation vì nó an toàn cho vật lý
            rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            // --- Di chuyển nhân vật ---
            // Di chuyển nhân vật theo hướng đã tính toán
            Vector3 moveVelocity = _moveDirection * moveSpeed;
            rb.velocity = new Vector3(moveVelocity.x, rb.velocity.y, moveVelocity.z);
        }
        else
        {
            // Nếu không có input, dừng di chuyển (giữ nguyên trục Y cho trọng lực)
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

        // 2. ÁP DỤNG LỰC NHẢY (Giữ nguyên)
        if (shouldJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); 
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            shouldJump = false; 
        }
    }
}