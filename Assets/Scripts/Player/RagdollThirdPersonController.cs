using UnityEngine;

public class RagdollThirdPersonController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody hips; // Rigidbody trung tâm ragdoll
    public Rigidbody controlSphere; // Rigidbody hình cầu điều khiển
    public Transform cameraTarget; // Target cho Cinemachine camera

    [Header("Movement Settings")]
    public float moveForce = 60f;
    public float sprintMultiplier = 1.5f;
    public float maxHorizSpeed = 7f;
    public float jumpImpulse = 7f;
    public float followForce = 60f;
    public float rotationLerp = 12f;

    [Header("Camera Settings")]
    public float mouseXSensitivity = 1.0f;
    public float mouseYSensitivity = 1.0f;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = false;

    float _cinemachineTargetYaw;
    float _cinemachineTargetPitch;
    const float _threshold = 0.01f;
    Camera _mainCam;
    Vector3 _lastMoveDir = Vector3.forward;
    bool grounded;

    void Awake()
    {
        if (!_mainCam) _mainCam = Camera.main;
    }

    void Start()
    {
        if (cameraTarget)
            _cinemachineTargetYaw = cameraTarget.rotation.eulerAngles.y;
        if (!hips || !controlSphere)
            Debug.LogWarning("[RagdollTPC] Chưa gán hips hoặc controlSphere.");
    }

    void Update()
    {
        CameraRotation();
        Jump();
    }

    void FixedUpdate()
    {
        Move();
        RotateTowardMoveDirection();
        ClampHorizontalSpeed();
        FollowSphere();
    }

    void Move()
    {
        if (!controlSphere || !_mainCam) return;
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 camForward = Vector3.Scale(_mainCam.transform.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight   = Vector3.Scale(_mainCam.transform.right,   new Vector3(1, 0, 1)).normalized;
        Vector3 moveDir = (camForward * v + camRight * h).normalized;
        float sprint = Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f;
        if (moveDir.sqrMagnitude > 0.01f)
        {
            controlSphere.AddForce(moveDir * moveForce * sprint, ForceMode.Acceleration);
            _lastMoveDir = moveDir;
        }
        // Nếu không có input, lấy hướng từ vận tốc sphere
        if (_lastMoveDir.sqrMagnitude <= 0.001f)
        {
            Vector3 vel = controlSphere.velocity;
            Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);
            if (horizVel.magnitude > 0.1f) _lastMoveDir = horizVel.normalized;
        }
    }

    void FollowSphere()
    {
        if (!hips || !controlSphere) return;
        Vector3 dirToSphere = controlSphere.position - hips.position;
        Vector3 horizontal = new Vector3(dirToSphere.x, 0f, dirToSphere.z);
        hips.AddForce(horizontal * followForce, ForceMode.Acceleration);
    }

    void RotateTowardMoveDirection()
    {
        if (!hips) return;
        Vector3 dir = Vector3.zero;
        Vector3 vel = controlSphere ? controlSphere.velocity : Vector3.zero;
        Vector3 horizVel = new Vector3(vel.x, 0f, vel.z);
        if (horizVel.sqrMagnitude > 0.0001f)
        {
            dir = horizVel.normalized;
            _lastMoveDir = dir;
        }
        if (dir.sqrMagnitude <= 0.001f)
            dir = _lastMoveDir;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            hips.MoveRotation(Quaternion.Slerp(hips.rotation, targetRot, rotationLerp * Time.fixedDeltaTime));
        }
    }

    void ClampHorizontalSpeed()
    {
        if (!controlSphere) return;
        Vector3 vel = controlSphere.velocity;
        Vector3 horiz = new Vector3(vel.x, 0, vel.z);
        if (horiz.magnitude > maxHorizSpeed)
        {
            Vector3 clamped = horiz.normalized * maxHorizSpeed;
            controlSphere.velocity = new Vector3(clamped.x, vel.y, clamped.z);
        }
    }

    void Jump()
    {
        if (!controlSphere) return;
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            controlSphere.AddForce(Vector3.up * jumpImpulse, ForceMode.Impulse);
        }
    }

    void CameraRotation()
    {
        if (!cameraTarget || LockCameraPosition) return;
        float lookX = Input.GetAxis("Mouse X") * mouseXSensitivity;
        float lookY = Input.GetAxis("Mouse Y") * mouseYSensitivity;
        Vector2 look = new Vector2(lookX, lookY);
        if (look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += look.x;
            _cinemachineTargetPitch += look.y;
        }
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
        cameraTarget.rotation = Quaternion.Euler(
            _cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }
}
