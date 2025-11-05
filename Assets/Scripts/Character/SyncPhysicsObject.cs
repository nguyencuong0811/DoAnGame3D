using UnityEngine;
public class SyncPhysicsObject : MonoBehaviour
{
    Rigidbody rigidbody3D;
    ConfigurableJoint joint;

    [SerializeField] Transform animateRef; // <== đổi từ Rigidbody sang Transform
    [SerializeField] bool syncAnimation = false;

    Quaternion startLocalRotation;

    void Awake()
    {
        rigidbody3D = GetComponent<Rigidbody>();
        joint = GetComponent<ConfigurableJoint>();
        // Tạm thời cứ lấy pose hiện tại, sẽ rebind lại ở Start
        startLocalRotation = transform.localRotation;
    }

    void Start()
    {
        // Sau khi Animator đã vào pose chuẩn (thường là ở Start),
        // chụp lại startLocalRotation để giảm lệch
        RebindStartPose();
        // (Tuỳ game, bạn có thể delay 1 frame bằng coroutine nếu cần.)
    }

    public void RebindStartPose()
    {
        startLocalRotation = transform.localRotation;
    }

    public void UpdateJointFromAnimation()
    {
        if (!syncAnimation || animateRef == null || joint == null) return;
        // joint phải ở local space (configuredInWorldSpace = false)
        ConfigurableJointExtensions.SetTargetRotationLocal(
            joint,
            animateRef.localRotation,
            startLocalRotation
        );
    }
}
