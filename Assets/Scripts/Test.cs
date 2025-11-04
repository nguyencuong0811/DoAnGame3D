using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class test : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;      // Kéo Main Camera vào đây
    public Transform groundCheck;          // Empty đặt dưới chân
    public LayerMask groundMask;

    [Header("Movement")]
    public float walkSpeed = 3.5f;
    public float sprintSpeed = 6.0f;
    [Range(0f, 20f)] public float acceleration = 10f;   // Tăng tốc mượt
    [Range(0f, 20f)] public float deceleration = 12f;   // Giảm tốc mượt
    public float rotationSpeed = 12f;                    // Quay mặt theo hướng chạy
    [Range(0f, 1f)] public float airControl = 0.5f;      // Điều khiển khi trên không

    [Header("Jump & Gravity")]
    public float jumpHeight = 1.6f;                      // Mét
    public float gravity = -18f;                         // Mạnh hơn -9.81 để rơi tự nhiên
    public float groundCheckRadius = 0.25f;              // Bán kính kiểm tra chạm đất
    public float groundedStick = -2f;                    // Dính đất nhẹ để không “bồng bềnh”

    private CharacterController cc;
    private float verticalVelocity;
    private Vector3 currentHorizontalVelocity;           // vận tốc ngang đang có
    private Vector3 desiredHorizontalVelocity;           // vận tốc ngang mong muốn
    private bool isGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 1) Ground check ổn định hơn cc.isGrounded
        isGrounded = Physics.CheckSphere(
            groundCheck.position, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

        // 2) Input (WASD) – có thể thay bằng Input System nếu bạn dùng
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool sprint = Input.GetKey(KeyCode.LeftShift);
        bool jumpPressed = Input.GetKeyDown(KeyCode.Space);

        // 3) Hướng di chuyển theo camera (bỏ component Y để không ngẩng/úp)
        Vector3 camForward = cameraTransform ? cameraTransform.forward : Vector3.forward;
        Vector3 camRight   = cameraTransform ? cameraTransform.right   : Vector3.right;
        camForward.y = 0f; camRight.y = 0f;
        camForward.Normalize(); camRight.Normalize();

        Vector3 moveInputDir = (camForward * v + camRight * h);
        moveInputDir = Vector3.ClampMagnitude(moveInputDir, 1f);

        // 4) Tốc độ mong muốn
        float targetSpeed = (sprint ? sprintSpeed : walkSpeed) * moveInputDir.magnitude;
        desiredHorizontalVelocity = moveInputDir * targetSpeed;

        // 5) Tăng/giảm tốc mượt (separate accel/decel)
        float lerpRate = (desiredHorizontalVelocity.magnitude > currentHorizontalVelocity.magnitude)
            ? acceleration : deceleration;

        // Trên không thì giảm khả năng điều khiển
        float control = isGrounded ? 1f : Mathf.Clamp01(airControl);
        currentHorizontalVelocity = Vector3.Lerp(
            currentHorizontalVelocity,
            desiredHorizontalVelocity,
            Mathf.Clamp01(lerpRate * control * Time.deltaTime)
        );

        // 6) Quay mặt theo hướng di chuyển (nếu có input)
        Vector3 flatVel = new Vector3(currentHorizontalVelocity.x, 0f, currentHorizontalVelocity.z);
        if (flatVel.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(flatVel, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // 7) Gravity & Jump
        if (isGrounded)
        {
            if (verticalVelocity < 0f) verticalVelocity = groundedStick; // dính đất
            if (jumpPressed)
            {
                // v = sqrt(2gh) với g âm
                verticalVelocity = Mathf.Sqrt(Mathf.Abs(2f * gravity * jumpHeight));
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // 8) Gộp chuyển động
        Vector3 velocity = currentHorizontalVelocity;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);
    }

    // Vẽ gizmo ground check cho dễ canh
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
