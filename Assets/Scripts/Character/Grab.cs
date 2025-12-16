using UnityEngine;

public class Grab : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;
    public Rigidbody handRigidbody;      // Rigidbody của tay
    public Transform grabPoint;          // Empty ở lòng bàn tay (tùy chọn)

    [Header("Input")]
    public int mouseButton = 0;          // 0 = LMB, 1 = RMB

    [Header("Joint")]
    public float breakForce = 3500f;
    public float breakTorque = 3500f;

    [Header("Weight Limit")]
    public bool enableWeightLimit = true;
    public float maxGrabMass = 10f;      // Khối lượng tối đa có thể cầm (kg)
    public float maxSkillMass = 20f;     // Khối lượng tối đa có thể cầm khi dùng skill (kg)
    public bool showWeightFeedback = true; // Hiện thông báo khi vật quá nặng
    public float timeskill = 15f; // Thời gian giữ skill
    public float speedOnGrab = 3f;
    private bool wantGrab;               // Đang giữ nút (muốn cầm)
    private GameObject candidate;        // Vật trong vùng tay
    private Collider candidateCol;       
    private GameObject grabbed;          // Vật đã cầm
    private FixedJoint joint;

    void Start()
    {
        if (!handRigidbody) handRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Bắt đầu muốn cầm => bật animation NGAY
        if (Input.GetMouseButtonDown(mouseButton))
        {
            wantGrab = true;
            if (animator) animator.SetBool("isGrabbing", true);
            // Nếu đã có đồ trong tay thì gắn luôn
            TryAttachIfPossible();
        }

        // Trong khi đang giữ nút: nếu vừa chạm đồ thì auto gắn
        if (wantGrab && joint == null && candidate != null)
        {
            TryAttachIfPossible();
        }

        // Thả nút tắt anim & thả đồ
        if (Input.GetMouseButtonUp(mouseButton))
        {
            wantGrab = false;
            if (animator) animator.SetBool("isGrabbing", false);
            RagdollController ragdoll = handRigidbody.GetComponentInParent<RagdollController>();
            ragdoll.ResetSpeed();
            Release();
        }
        if(Input.GetKeyDown(KeyCode.G))
        {
            UseSkillGrab();
        }
    }

    void TryAttachIfPossible()
    {
        if (candidate == null || joint != null) return;

        Rigidbody targetRb = candidate.GetComponent<Rigidbody>();
        if (targetRb == null || targetRb.isKinematic) return;

        // KIỂM TRA CÂN NẶNG
        if (enableWeightLimit && targetRb.mass > maxGrabMass)
        {
            if (showWeightFeedback)
            {
                Debug.Log($"Vật {candidate.name} quá nặng! ({targetRb.mass:F1}kg > {maxGrabMass}kg)");
                //thêm UI notification hoặc sound effect ở đây
            }
        }
        if(targetRb.mass > maxGrabMass)
        {
            RagdollController ragdoll = handRigidbody.GetComponentInParent<RagdollController>();
            Debug.Log($"Giảm tốc độ vì cầm vật nặng {targetRb.mass}kg");
            ragdoll.SetSpeedOnGrab(speedOnGrab);
        }
        // Tạo joint trên VẬT và nối với tay
        joint = candidate.AddComponent<FixedJoint>();
        joint.connectedBody = handRigidbody;
        joint.enableCollision = true;
        joint.breakForce = breakForce;
        joint.breakTorque = breakTorque;

        // (Tùy chọn) đặt anchor chính xác để ít rung
        joint.autoConfigureConnectedAnchor = false;
        Vector3 handAttach = grabPoint ? grabPoint.position : transform.position;
        Vector3 objAttach = candidateCol ? candidateCol.ClosestPoint(handAttach) : candidate.transform.position;
        joint.anchor = candidate.transform.InverseTransformPoint(objAttach);
        joint.connectedAnchor = handRigidbody.transform.InverseTransformPoint(handAttach);

        grabbed = candidate;
    }

    void Release()
    {
        if (joint != null)
        {
            // Truyền vận tốc tay để thả tự nhiên
            var rb = grabbed ? grabbed.GetComponent<Rigidbody>() : null;
            Vector3 v = handRigidbody.velocity;
            Vector3 w = handRigidbody.angularVelocity;

            Destroy(joint);
            joint = null;

            if (rb != null)
            {
                rb.velocity = v;
                rb.angularVelocity = w;
            }
        }

        grabbed = null;
        // Giữ candidate để nếu vẫn chạm và bạn bấm lại thì vẫn lấy được
    }

    // Tay là Capsule Collider (IsTrigger = true)
    private void OnTriggerStay(Collider other)
    {
        // Chỉ nhận đồ có Rigidbody
        if (other.attachedRigidbody == null) return;

        // Bạn có thể đổi sang kiểm tra Tag/Layer tuỳ setup
        candidate = other.attachedRigidbody.gameObject;
        candidateCol = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (candidateCol == other)
        {
            candidate = null;
            candidateCol = null;
        }
    }

    // HÀM HỖ TRỢ: Kiểm tra xem có thể cầm vật không
    public bool CanGrabObject(GameObject obj)
    {
        if (obj == null) return false;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb == null || rb.isKinematic) return false;

        if (enableWeightLimit && rb.mass > maxGrabMass) return false;

        return true;
    }
    public void UseSkillGrab()
    {
        maxGrabMass = maxSkillMass;
        Invoke("ResetGrabMass", timeskill);
    }
    void ResetGrabMass()
    {
        maxGrabMass = 10f;
        // Nếu đang cầm đồ
        if (grabbed != null)
        {
            Rigidbody rb = grabbed.GetComponent<Rigidbody>();

            // mà đồ nặng quá thì thả
            if (rb != null && rb.mass > maxGrabMass)
            {
                Debug.Log($"Thả vì {grabbed.name} quá nặng sau khi hết skill!");
                Release();
            }
        }
    }
}