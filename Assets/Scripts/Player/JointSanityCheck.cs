using UnityEngine;

public class JointSanityCheck : MonoBehaviour
{
    void Start()
    {
        var rb = GetComponent<Rigidbody>();
        var j = GetComponent<Joint>();
        if (!rb) Debug.LogError("[Hips] Thiếu Rigidbody!");
        if (!j) Debug.LogError("[Hips] Thiếu Joint (Fixed/Configurable)!");
        else if (!j.connectedBody) Debug.LogError("[Hips] Joint chưa gán Connected Body (phải là RB của Sphere)!");
        if (rb && rb.isKinematic) Debug.LogWarning("[Hips] Rigidbody đang IsKinematic = true (hãy tắt).");
    }
}
