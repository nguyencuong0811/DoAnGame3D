using UnityEngine;

public class RagdollController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveForce = 500f;
    [SerializeField] private float runForceMultiplier = 1.5f;
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float maxRunSpeed = 12f;
    [SerializeField] private float drag = 2f;
    
    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 300f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpCooldown = 0.3f;
    
    [Header("Ragdoll References")]
    [SerializeField] private Rigidbody sphereRigidbody; // AnimateBody (sphere)
    [SerializeField] private ConfigurableJoint hipJoint; // ConfigurableJoint của hips
    
    [Header("Animation Settings")]
    [SerializeField] private Animator animator; // THÊM ANIMATOR
    [SerializeField] private float animationSpeedMultiplier = 0.4f; // Điều chỉnh tốc độ animation
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float jointRotationDrive = 1000f;
    [SerializeField] private float jointRotationDamping = 100f;
    
    // Movement variables
    private bool isGrounded;
    private float moveX;
    private float moveZ;
    private bool isRunning;
    private bool jumpPressed;
    private Vector3 lastMoveDirection;
    private Quaternion targetRotation;

    // Pickup system reference (optional)
    private MovingOutStylePickup pickupSystem;
    private float lastJumpTime;

    // Animation sync system
    private SyncPhysicsObject[] syncPhysicsObjects; // THÊM SYNC SYSTEM

    //Chap va :((
    private float _maxSpeed;
    private float _maxRunSpeed;
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
        }
        
        // Setup ConfigurableJoint rotation drive
        if (hipJoint != null)
        {
            hipJoint.rotationDriveMode = RotationDriveMode.Slerp;
            
            JointDrive slerpDrive = new JointDrive();
            slerpDrive.positionSpring = jointRotationDrive;
            slerpDrive.positionDamper = jointRotationDamping;
            slerpDrive.maximumForce = Mathf.Infinity;
            hipJoint.slerpDrive = slerpDrive;
            
            Transform sphereTransform = hipJoint.connectedBody != null ? hipJoint.connectedBody.transform : transform;
            Quaternion worldRotation = hipJoint.transform.rotation;
            targetRotation = Quaternion.Inverse(sphereTransform.rotation) * worldRotation;
        }
        
        // Tìm pickup system nếu có
        pickupSystem = GetComponentInChildren<MovingOutStylePickup>();
        
        // TÌM TẤT CẢ SYNCPHYSICSOBJECT COMPONENTS (giống NetworkPlayer)
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
        
        lastMoveDirection = transform.forward;

        _maxSpeed = maxSpeed;
        _maxRunSpeed = maxRunSpeed;
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        HandleRotation();
        HandleJump();
        UpdateAnimation();
    }

    void HandleInput()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        moveZ = Input.GetAxisRaw("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetButtonDown("Jump"))
            jumpPressed = true;
    }

    void CheckGround()
    {
        if (groundCheck == null) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    void HandleMovement()
    {
        if (sphereRigidbody == null) return;
        
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        Vector3 moveDirection = cameraRight * moveX + cameraForward * moveZ;
        moveDirection = moveDirection.normalized;
        
        if (moveDirection.magnitude > 0.1f)
        {
            lastMoveDirection = moveDirection;
        }
        
        float currentMaxSpeed = isRunning ? maxRunSpeed : maxSpeed;
        
        if (pickupSystem != null && pickupSystem.IsCarrying())
        {
            currentMaxSpeed *= 0.6f;
        }
        
        Vector3 horizontalVelocity = new Vector3(sphereRigidbody.velocity.x, 0, 
                                                  sphereRigidbody.velocity.z);
        
        if (horizontalVelocity.magnitude < currentMaxSpeed)
        {
            float force = isRunning ? moveForce * runForceMultiplier : moveForce;
            sphereRigidbody.AddForce(moveDirection * force * Time.fixedDeltaTime, 
                                     ForceMode.Force);
        }
        
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
        
        Quaternion worldTargetRotation = Quaternion.LookRotation(lastMoveDirection);
        
        Transform sphereTransform = hipJoint.connectedBody != null ? hipJoint.connectedBody.transform : transform;
        Quaternion localTargetRotation = Quaternion.Inverse(sphereTransform.rotation) * worldTargetRotation;
        
        if (moveX != 0 || moveZ != 0)
        {
            targetRotation = Quaternion.Slerp(targetRotation, localTargetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        
        hipJoint.targetRotation = Quaternion.Inverse(targetRotation);
    }

    void HandleJump()
    {
        if (sphereRigidbody == null) return;

        bool canJump = Time.time - lastJumpTime > jumpCooldown;

        if (jumpPressed && isGrounded && canJump)
        {
            AudioManager.Instance.PlaySFXJump();
            sphereRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJumpTime = Time.time;
        }

        jumpPressed = false;
    }

    // HÀM MỚI: CẬP NHẬT ANIMATION (giống NetworkPlayer)
    void UpdateAnimation()
    {
        if (animator == null) return;

        // Tính local forward velocity (giống NetworkPlayer line 124)
        Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, sphereRigidbody.velocity);
        float localForwardVelocity = localVelocityVsForward.magnitude;

        // Set animation speed parameter
        animator.SetFloat("movementSpeed", localForwardVelocity * animationSpeedMultiplier);

        // Có thể thêm các parameter khác nếu cần:
        

        // UPDATE JOINT ROTATION FROM ANIMATION (giống NetworkPlayer line 127-130)
        if (syncPhysicsObjects != null)
        {
            for (int i = 0; i < syncPhysicsObjects.Length; i++)
            {
                syncPhysicsObjects[i].UpdateJointFromAnimation();
            }
        }
    }

    // PUBLIC METHODS
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
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        
        if (sphereRigidbody != null)
        {
            if (Application.isPlaying && lastMoveDirection.magnitude > 0.1f)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(sphereRigidbody.position, lastMoveDirection * 2f);
            }
        }
        
        if (hipJoint != null && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Vector3 targetForward = targetRotation * Vector3.forward;
            Gizmos.DrawRay(hipJoint.transform.position, targetForward * 1.5f);
        }
    }
    public void SetSpeedOnGrab(float speed)
    {
        maxSpeed = speed;
        maxRunSpeed = speed * 1.5f;
    }
    public void ResetSpeed()
    {
        maxSpeed = _maxSpeed;
        maxRunSpeed = _maxRunSpeed;
    }
}