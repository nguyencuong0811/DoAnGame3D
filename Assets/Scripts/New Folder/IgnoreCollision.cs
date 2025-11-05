using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollision : MonoBehaviour
{
    [SerializeField]
    Collider thisColliders;

    [SerializeField]
    Collider[] collidersToIgnore;
    void Start()
    {
        foreach (Collider otherCollider in collidersToIgnore)
        {
            Physics.IgnoreCollision(thisColliders, otherCollider, true);
        }
    }
}
