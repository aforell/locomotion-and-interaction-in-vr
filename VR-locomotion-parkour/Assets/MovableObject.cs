using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovableObject : MonoBehaviour, Movable
{
    private Transform originalParent;
    private Rigidbody rb;
    private bool isKinematic = false;
    public float cooldownTimer = 0f;
    public float cooldownTime = 0.4f;

    void Start ()
    {
        originalParent = transform.parent;
        rb = gameObject.GetComponent<Rigidbody>();
        isKinematic = rb == null ? false : rb.isKinematic;
    }

    void Update ()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public void attach (Transform hook, GrapplingHook gp)
    {
        if (cooldownTimer > 0)
        {
            return;
        }
        gp.attachedMovables.Add(this);

        transform.SetParent(hook);
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // layer 11 = HookObjects
        gameObject.layer = 11;
    }

    public void detach (GrapplingHook gp)
    {
        Debug.Log("detach: " + gameObject.name);
        transform.SetParent(originalParent);
        if (rb != null)
        {
            rb.isKinematic = isKinematic;
        }

        // layer 0 = Default
        gameObject.layer = 0;
        cooldownTimer = cooldownTime;
    }
}
