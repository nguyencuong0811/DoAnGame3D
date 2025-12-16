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
        //assune that we are not grounded
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

        float inputManitude = moveInputVector.magnitude;

        Vector3 localVelocityVsForward = transform.forward * Vector3.Dot(transform.forward, rigidbody3D.velocity);

        float localFowardVelocity = localVelocityVsForward.magnitude;

        //do toc do theo mat phang
        Vector3 planarVel = Vector3.ProjectOnPlane(rigidbody3D.velocity, Vector3.up);
        float speed = planarVel.magnitude;

        if (inputManitude != 0)
        {
            Quaternion desiredDirection = Quaternion.LookRotation(new Vector3(moveInputVector.x, 0, moveInputVector.y * -1f), transform.up);

            //rotate towards desired direction
            mainJoint.targetRotation = Quaternion.RotateTowards(mainJoint.targetRotation, desiredDirection, Time.deltaTime * 200f);

            if (speed < maxSpeed)
            {
                // move the character in the direction it is facing
                Vector3 desiredDir = new Vector3(-moveInputVector.x, 0f, -moveInputVector.y).normalized;
                rigidbody3D.AddForce(desiredDir * inputManitude * 40f);
            }
        }

        if (isGrounded && isJumpButtonPressed)
        {
            rigidbody3D.AddForce(transform.up * 20f, ForceMode.Impulse);
            isJumpButtonPressed = false;
        }

        animator.SetFloat("movementSpeed", localFowardVelocity * 0.4f);
        
        //update the joint rotation based on animation
        for (int i = 0; i < syncPhysicsObjects.Length; i++)
        {
            syncPhysicsObjects[i].UpdateJointFromAnimation();
        }
    }
}
