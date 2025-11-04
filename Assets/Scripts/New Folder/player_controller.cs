using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerTopDown : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float acceleration = 10f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    // Components
    private CharacterController controller;
    private MovingOutStylePickup pickupSystem;
    
    // Movement variables
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    
    // Input
    private float moveX;
    private float moveZ;
    private bool isRunning;
    private bool jumpPressed;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        pickupSystem = GetComponent<MovingOutStylePickup>();
        
        // Setup camera nếu chưa có
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Tạo ground check nếu chưa có
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -controller.height / 2, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        currentSpeed = walkSpeed;
    }

    void Update()
    {
        HandleInput();
        HandleMovement();
        HandleJump();
        HandleGravity();
    }

    void HandleInput()
    {
        // WASD / Arrow Keys
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");
        
        // Shift để chạy
        isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Space để nhảy
        jumpPressed = Input.GetButtonDown("Jump");
    }

    void HandleMovement()
    {
        // Kiểm tra chạm đất
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Tính hướng di chuyển THEO HƯỚNG CAMERA
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        // Bỏ thành phần Y để chỉ di chuyển trên mặt phẳng ngang
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Hướng di chuyển tương đối với camera
        Vector3 moveDirection = cameraRight * moveX + cameraForward * moveZ;
        
        // Xoay player theo hướng di chuyển
        if (moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Tính tốc độ
        float targetSpeed = walkSpeed;
        
        if (isRunning && isGrounded)
        {
            targetSpeed = runSpeed;
        }
        
        // Giảm tốc độ nếu đang cầm đồ
        if (pickupSystem != null && pickupSystem.IsCarrying())
        {
            targetSpeed *= 0.6f;
        }
        
        // Smooth acceleration
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        
        // Di chuyển
        Vector3 move = moveDirection.normalized * currentSpeed;
        controller.Move(move * Time.deltaTime);
    }

    void HandleJump()
    {
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    void HandleGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        walkSpeed *= multiplier;
        runSpeed *= multiplier;
    }

    public bool IsMoving()
    {
        return moveX != 0 || moveZ != 0;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}