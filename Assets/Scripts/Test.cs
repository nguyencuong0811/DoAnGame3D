using UnityEngine;

public class test : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 500f;
    [SerializeField] private float runForceMultiplier = 1.5f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxRunSpeed = 12f;
    [SerializeField] private float drag = 2f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 300f;
    [SerializeField] private Transform groundCheck; // Empty object dưới chân
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpCooldown = 0.3f;
    
    [Header("Ragdoll References")]
    [SerializeField] private Rigidbody sphereRigidbody; // AnimateBody (sphere)
    [SerializeField] private ConfigurableJoint hipJoint; // ConfigurableJoint của hips
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f; // Tốc độ xoay mục tiêu
    [SerializeField] private float jointRotationDrive = 1000f; // Sức mạnh xoay của joint
    [SerializeField] private float jointRotationDamping = 100f; // Damping cho xoay
    
    // Movement variables
    private bool isGrounded;
    private float moveX;
    private float moveZ;
    private bool isRunning;
    private bool jumpPressed;
    private Vector3 lastMoveDirection;
    private Quaternion targetRotation;
    private float lastJumpTime;
    
    // Pickup system reference (optional)
    private MovingOutStylePickup pickupSystem;

    void Start()
    {
        // Script này gắn vào AnimateBody (sphere)
        if (sphereRigidbody == null)
        {
            sphereRigidbody = GetComponent<Rigidbody>();
        }
        
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // Setup sphere rigidbody
        if (sphereRigidbody != null)
        {
            sphereRigidbody.drag = drag;
            sphereRigidbody.mass = 10f;
            sphereRigidbody.interpolation = RigidbodyInterpolation.Interpolate; // Smooth movement
            sphereRigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision
        }
        
        // Setup ConfigurableJoint rotation drive
        if (hipJoint != null)
        {
            // Đảm bảo rotation mode đúng
            hipJoint.rotationDriveMode = RotationDriveMode.Slerp;
            
            // Bật Slerp Drive để điều khiển rotation
            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = jointRotationDrive;
            slerpDrive.positionDamper = jointRotationDamping;
            slerpDrive.maximumForce = Mathf.Infinity;
            hipJoint.slerpDrive = slerpDrive;
            
            // Khởi tạo target rotation = rotation hiện tại (world space)
            Transform sphereTransform = hipJoint.connectedBody != null ? hipJoint.connectedBody.transform : transform;
            Quaternion worldRotation = hipJoint.transform.rotation;
            targetRotation = Quaternion.Inverse(sphereTransform.rotation) * worldRotation;
        }
        
        // Tìm pickup system nếu có
        pickupSystem = GetComponentInChildren<MovingOutStylePickup>();
        
        lastMoveDirection = transform.forward;
    }

    void Update()
    {
        HandleInput();
        CheckGround();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
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

    void CheckGround()
    {
        if (groundCheck == null) return;
        
        // Dùng OverlapSphere tại vị trí groundCheck
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    void HandleMovement()
    {
        if (sphereRigidbody == null) return;
        
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
        moveDirection = moveDirection.normalized;
        
        // Lưu hướng di chuyển cuối cùng để xoay
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;
        }
        
        // Tính tốc độ tối đa
        float currentMaxSpeed = isRunning ? maxRunSpeed : maxSpeed;
        
        // Giảm tốc độ nếu đang cầm đồ
        if (pickupSystem != null && pickupSystem.IsCarrying())
        {
            currentMaxSpeed *= 0.6f;
        }
        
        // Chỉ áp dụng force nếu chưa đạt max speed
        Vector3 horizontalVelocity = new Vector3(sphereRigidbody.velocity.x, 0, 
                                                  sphereRigidbody.velocity.z);
        
        if (horizontalVelocity.magnitude < currentMaxSpeed)
        {
            float force = isRunning ? moveForce * runForceMultiplier : moveForce;
            sphereRigidbody.AddForce(moveDirection * force * Time.fixedDeltaTime, 
                                     ForceMode.Force);
        }
        
        // Giới hạn vận tốc ngang
        if (horizontalVelocity.magnitude > currentMaxSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * currentMaxSpeed;
            sphereRigidbody.velocity = new Vector3(horizontalVelocity.x, 
                                                   sphereRigidbody.velocity.y, 
                                                   horizontalVelocity.z);
        }
    }

    void HandleRotation()
    {
        if (hipJoint == null) return;
        
        // Luôn cập nhật rotation, không chỉ khi di chuyển
        // Tính world rotation mục tiêu
        Quaternion worldTargetRotation = Quaternion.LookRotation(lastMoveDirection);
        
        // Chuyển sang local rotation so với sphere (connected body)
        Transform sphereTransform = hipJoint.connectedBody != null ? hipJoint.connectedBody.transform : transform;
        Quaternion localTargetRotation = Quaternion.Inverse(sphereTransform.rotation) * worldTargetRotation;
        
        // Lerp smooth đến target rotation (chỉ khi đang di chuyển)
        if (moveX != 0 || moveZ != 0)
        {
            targetRotation = Quaternion.Slerp(targetRotation, localTargetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        
        // Set target rotation cho joint (inverse vì ConfigurableJoint dùng inverse quaternion)
        hipJoint.targetRotation = Quaternion.Inverse(targetRotation);
    }

    void HandleJump()
    {
        if (sphereRigidbody == null) return;
        
        // Kiểm tra cooldown
        bool canJump = Time.time - lastJumpTime > jumpCooldown;
        
        if (jumpPressed && isGrounded && canJump)
        {
            // Reset velocity.y trước khi nhảy
            Vector3 vel = sphereRigidbody.velocity;
            vel.y = 0;
            sphereRigidbody.velocity = vel;
            
            // Apply jump force
            sphereRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // Lưu thời gian nhảy
            lastJumpTime = Time.time;
        }
    }

    public bool IsMoving()
    {
        return moveX != 0 || moveZ != 0;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetCurrentSpeed()
    {
        if (sphereRigidbody == null) return 0;
        Vector3 horizontalVelocity = new Vector3(sphereRigidbody.velocity.x, 0, 
                                                  sphereRigidbody.velocity.z);
        return horizontalVelocity.magnitude;
    }

    // Điều chỉnh Joint Drive từ code khác nếu cần
    public void SetJointRotationStrength(float spring, float damper)
    {
        if (hipJoint == null) return;
        
        JointDrive drive = hipJoint.slerpDrive;
        drive.positionSpring = spring;
        drive.positionDamper = damper;
        hipJoint.slerpDrive = drive;
    }

    void OnDrawGizmos()
    {
        // Vẽ ground check sphere
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        if (sphereRigidbody != null)
        {
            // Vẽ hướng di chuyển
            if (Application.isPlaying && lastMoveDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(sphereRigidbody.position, lastMoveDirection * 2f);
            }
        }
        
        // Vẽ hướng hips target
        if (hipJoint != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 targetForward = targetRotation * Vector3.forward;
            Gizmos.DrawRay(hipJoint.transform.position, targetForward * 1.5f);
        }
    }
}