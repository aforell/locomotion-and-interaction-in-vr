using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
    public bool isLeftHandCollidingWithHook;
    public bool isRightHandCollidingWithHook;
    public bool isObjectMode;
    public bool isRigidMode;
    public Rigidbody rb;
    public List<Movable> attachedMovables = new List<Movable>();
    public bool isHookOnGround;
    private float timeSinceCollision = 0.1f;
    public bool isGrabbed = false;

    public void Start ()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Update ()
    {
        if (timeSinceCollision > 0)
        {
            timeSinceCollision -= Time.deltaTime;
        }
        else
        {
            isHookOnGround = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // layer 9 is Hands
        if (other.gameObject.layer == 9)
        {
            if (other.gameObject.tag == "left")
            {
                isLeftHandCollidingWithHook = true;
            }
            else if (other.gameObject.tag == "right")
            {
                isRightHandCollidingWithHook = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // layer 9 is Hands
        if (other.gameObject.layer == 9)
        {
            if (other.gameObject.tag == "left")
            {
                isLeftHandCollidingWithHook = false;
            }
            else if (other.gameObject.tag == "right")
            {
                isRightHandCollidingWithHook = false;
            }
        }
    }

    void OnCollisionStay(Collision other)
    {
        timeSinceCollision = 0.1f;
        isHookOnGround = true;
    }

    public void switchToRigidMode (bool isNowRigidMode)
    {
        if (isNowRigidMode)
        {
            simulateSetKinematic(true);
        }
        else
        {
            simulateSetKinematic(false);
        }
        isRigidMode = isNowRigidMode;
    }

    public void simulateSetKinematic (bool simulateIsKinematic)
    {
        if (simulateIsKinematic)
        {
            rb.useGravity = false;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.angularDrag = Mathf.Infinity;
        }
        else
        {
            rb.useGravity = true;
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            rb.angularDrag = 0.05f;
        }
    }

    public void switchToObjectMode (bool isNowObjectMode)
    {
        // everything that collides and is movable should now become a child of the hook
        isObjectMode = isNowObjectMode;
        if (!isObjectMode)
        {
            detachEverything();
        }
    }

    public void detachEverything ()
    {
        foreach (Movable movable in attachedMovables)
        {
            movable.detach(this);
        }
        attachedMovables.Clear();
    }

    void OnCollisionEnter (Collision other)
    {
        if (!isObjectMode)
        {
            return;
        }

        Movable otherMovable = other.gameObject.GetComponent<Movable>();
        if (otherMovable != null)
        {
            if (!attachedMovables.Contains(otherMovable))
            {
                otherMovable.attach(transform, this);
            }
        }
    }

    public bool isSomethingAttached ()
    {
        return attachedMovables.Count > 0;
    }

    public void OnGrab()
    {
        isGrabbed = true;
    }

    public void OnRelease()
    {
        isGrabbed = false;
    }
}
