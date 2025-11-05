using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    [SerializeField]
    Rigidbody rigidbody3D;

    [SerializeField]
    ConfigurableJoint mainJoint;

    [SerializeField]
    Animator animator;

    [Header("Camera Reference")]
    [SerializeField]
    Transform cameraTransform; // THÊM REFERENCE ĐỂ DI CHUYỂN THEO CAMERA

    //Input
    Vector2 moveInputVector = Vector2.zero;
    bool isJumpButtonPressed = false;

    //Controller settings
    float maxSpeed = 5f;

    //States
    bool isGrounded = false;

    //Raycast
    RaycastHit[] raycastHits = new RaycastHit[10];

    //syncing of physics obj
    SyncPhysicsObject[] syncPhysicsObjects;

    void Awake()
    {
        syncPhysicsObjects = GetComponentsInChildren<SyncPhysicsObject>();
    }
    
    void Start()
    {
        // Tự động lấy main camera nếu chưa assign
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        moveInputVector.x = Input.GetAxis("Horizontal");
        moveInputVector.y = Input.GetAxis("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumpButtonPressed = true;
        }
    }
    
    void FixedUpdate()
    {
        //assume that we are not grounded
        isGrounded = false;

        //check if we are grounded
        int numberOfHits = Physics.SphereCastNonAlloc(rigidbody3D.position, 0.1f, transform.up * -1f, raycastHits, 0.5f);

        //check for valid hits
        for (int i = 0; i < numberOfHits; i++)
        {
            //ignore self collisions
            if (raycastHits[i].transform.root == transform)
            {
                continue;
            }
            isGrounded = true;
            break;
        }

        //apply extra gravity to character to make it less floaty
        if (!isGrounded)
            rigidbody3D.AddForce(Vector3.down * 10);

        float inputMagnitude = moveInputVector.magnitude;

        Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.velocity);
        float localForwardVelocity = localVelocityVsForward.magnitude;

        //tốc độ theo mặt phẳng
        Vector3 planarVel = Vector3.ProjectOnPlane(rigidbody3D.velocity, Vector3.up);
        float speed = planarVel.magnitude;

        if (inputMagnitude != 0)
        {
            // TÍNH HƯỚNG DI CHUYỂN THEO CAMERA (GIỐNG RAGDOLLCONTROLLER)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            // Bỏ thành phần Y để chỉ di chuyển trên mặt phẳng ngang
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Hướng di chuyển tương đối với camera
            Vector3 moveDirection = cameraRight * moveInputVector.x + cameraForward * moveInputVector.y;
            moveDirection = moveDirection.normalized;

            // SỬA: Bỏ dấu trừ (-) không cần thiết
            Quaternion desiredDirection = Quaternion.LookRotation(moveDirection, transform.up);

            //rotate towards desired direction
            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.deltaTime * 200f);

            if (speed < maxSpeed)
            {
                // SỬA: Bỏ dấu trừ (-), dùng moveDirection từ camera
                rigidbody3D.AddForce(moveDirection * inputMagnitude * 40f);
            }
        }

        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(transform.up * 20f, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }

        animator.SetFloat("movementSpeed", localForwardVelocity * 0.4f);
        
        //update the joint rotation based on animation
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}