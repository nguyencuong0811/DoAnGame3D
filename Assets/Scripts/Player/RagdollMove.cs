using UnityEngine;

public class RagdollMove : MonoBehaviour
{
    public float moveForce = 10f;
    public float rotateSpeed = 5f;
    public Transform hips; // gán hips của ragdoll
    public Camera cam;
    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        // Hướng theo camera
        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // Di chuyển sphere bằng lực
        rb.AddForce(moveDir * moveForce, ForceMode.Acceleration);

        // Xoay hips theo hướng di chuyển
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            hips.rotation = Quaternion.Slerp(hips.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
        }
    }
}
