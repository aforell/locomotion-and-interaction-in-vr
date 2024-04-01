using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XRTools.Rendering;
using UnityEngine.InputSystem;
using System;
using Oculus.Interaction.HandGrab;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Mathematics;
using Unity.XR.CoreUtils;

public class Rope : MonoBehaviour
{
    public float pullSpeed = 1f;
    public float rotationSpeed = 1f;
    public CharacterController playerController;
    public Transform hookTransform;
    public bool isObjectMode = false;
    public bool isRigidMode = false;
    public InputAction leftGrabAction;
    public InputAction leftReleaseAction;
    public InputAction rightGrabAction;
    public InputAction rightReleaseAction;
    public InputAction leftTrigger;
    public InputAction rightTrigger;
    public InputAction optionButton;
    public LineRenderer Line;
    public Transform PlayerAttachPoint;
    public Transform RightHand;
    public Transform LeftHand;

    public float timeSinceLeftCollided = 1f;
    public float timeSinceRightCollided = 1f;
    private bool isLeftAttached = false;
    private bool isRightAttached = false;
    private bool isLeftAttachmentNearPlayer = true;
    private Vector3 firstLeftAttachmentPosition = Vector3.zero;
    private Vector3 firstRightAttachmentPosition = Vector3.zero;
    private Vector3 lastLeftHandPosition = Vector3.zero;
    private Vector3 lastRightHandPosition = Vector3.zero;
    private Quaternion lastLeftHandRotation = Quaternion.identity;
    private Quaternion lastRightHandRotation = Quaternion.identity;
    


    private GrapplingHook grapplingHook;
    private ContinuousMoveProviderBase playerMoveProvider;
    private float currentRadius;
    [SerializeField] private Material rigidMaterial;
    [SerializeField] private Material nonRigidMaterial;
    [SerializeField] private GameObject [] objectmodeIndicatorGroup;

    public GameObject OptionsMenu;
    public void Start ()
    {
        grapplingHook = hookTransform.gameObject.GetComponent<GrapplingHook>();
        playerMoveProvider = playerController.gameObject.GetComponent<ActionBasedContinuousMoveProvider>();
        Line.material = isRigidMode ? rigidMaterial : nonRigidMaterial;
        foreach (GameObject go in objectmodeIndicatorGroup)
        {
            go.SetActive(isObjectMode);
        }
    }

    public void Update()
    {
        timeSinceLeftCollided += Time.deltaTime;
        timeSinceRightCollided += Time.deltaTime;
        Debug.Log("Height: " + playerController.height);
        // Debug.Log("fly: " + playerMoveProvider.enableFly);
        // Debug.Log("left: " + isLeftHandCollidingWithRope());
        // Debug.Log("right: " + isRightHandCollidingWithRope());
        // Debug.Log("leftAttached: " + isLeftAttached);
        // Debug.Log("rightAttached: " + isRightAttached);
    }

    void FixedUpdate()
    {
        setLinePositionsWithHands();
        GenerateMeshCollider();
        HandleHandMovement();
    }

    public void HandleHandMovement ()
    {
        if (grapplingHook.isGrabbed)
        {
            return;
        }
        bool isLeftSignificantHand = isLeftAttached && !isRightAttached || isLeftAttached && !isLeftAttachmentNearPlayer;
        bool isRightSignificantHand = isRightAttached && !isLeftAttached || isRightAttached && isLeftAttachmentNearPlayer;
        // get hand movement data
        Vector3 ropeLine = hookTransform.position - PlayerAttachPoint.position;
        Vector3 deltaLeftPos = lastLeftHandPosition - LeftHand.position;
        Vector3 deltaRightPos = lastRightHandPosition - RightHand.position;

        Vector3 leftMove = Vector3.Project(deltaLeftPos, ropeLine) * pullSpeed;
        Vector3 rightMove = Vector3.Project(deltaRightPos, ropeLine) * pullSpeed;

        grapplingHook.rb.drag = 0;
        grapplingHook.rb.useGravity = true;
        playerMoveProvider.enableFly = (isRigidMode && !isObjectMode);
        playerMoveProvider.useGravity = !playerMoveProvider.enableFly;

        // move player / hook, in rigid mode only when not both hands are on the rope to avoid conflict with rotation
        if (!isRigidMode || !isLeftAttached || !isRightAttached)
        {
            if (isObjectMode)
            {
                leftMove = leftMove * -1;
                rightMove = rightMove * -1;
            }
            else if(!isRigidMode)
            {
                // hardcode fix bug where you fly off when pushing on top of hook
                // determine if you are pushing via the angle
                float angleL = Vector3.Angle(ropeLine, leftMove);
                float angleR = Vector3.Angle(ropeLine, rightMove);
                if(angleL > 2)
                {
                    // then delete the y component of the move direction
                    leftMove = new Vector3(leftMove.x, 0, leftMove.z);
                }
                if(angleR > 2)
                {
                    rightMove = new Vector3(rightMove.x, 0, rightMove.z);
                }
                
            }
        
            if (isLeftSignificantHand)
            {
                Move(leftMove);
            }
            else if (isRightSignificantHand)
            {
                Move(rightMove);
            }
        }

        if(isRigidMode && isObjectMode)
        {
            grapplingHook.rb.velocity = Vector3.zero;
            grapplingHook.rb.drag = Mathf.Infinity;
            grapplingHook.rb.useGravity = false;
        }

        // rotate hook around player in rigid mode only when both hands are attached
        if (isRigidMode && isLeftAttached && isRightAttached)
        {
            Vector3 goalPosition = Vector3.zero;
            Vector3 frontHandPos = Vector3.zero;
            Vector3 backHandPos = Vector3.zero;
            if (!isLeftAttachmentNearPlayer)
            {
                // fronthand = near hook
                frontHandPos = LeftHand.position;
                backHandPos = RightHand.position;
            }
            else
            {
                frontHandPos = RightHand.position;
                backHandPos = LeftHand.position;
            }

            // rigid movement
            // rigid object movement
            if (isObjectMode)
            {
                Vector3 goalDirection = frontHandPos - backHandPos;
                goalPosition = backHandPos + goalDirection.normalized * currentRadius;
                // small steps to have animation and no snapping
                if (Vector3.Distance(hookTransform.position, goalPosition) > 0.0005f)
                {
                    Vector3 moveToGoalDirection = hookTransform.position - goalPosition;
                    grapplingHook.rb.drag = 0;
                    grapplingHook.rb.velocity = - moveToGoalDirection * rotationSpeed;
                    //hookTransform.position -= moveToGoalDirection * rotationSpeed;
                }
            }
            // rigid player movement
            else
            {
                Vector3 goalDirection = backHandPos - frontHandPos;
                goalPosition = hookTransform.position + goalDirection.normalized * currentRadius;
                // small steps to have animation and no snapping
                if (Vector3.Distance(playerController.transform.position, goalPosition) > 0.0005f && Vector3.Distance(playerController.transform.position, hookTransform.position) < 2 * currentRadius)
                {
                    Vector3 moveToGoalDirection = playerController.transform.position - goalPosition;
                    playerController.Move(- moveToGoalDirection * 0.5f);
                }
            }

            // hook rotation
            // hook always points in the direction of the rope
            // Quaternion rotation = Quaternion.FromToRotation(hookTransform.up, (frontHandPos - backHandPos).normalized) * hookTransform.rotation;

            Transform fronthand = isLeftAttachmentNearPlayer ? RightHand : LeftHand;

            // Attempt to have rotation not exactly like frontHand
            // Quaternion rotation2 = hookTransform.rotation;
            // rotation2.SetLookRotation(fronthand.up, (frontHandPos - backHandPos).normalized);

            if(isObjectMode)
            {
                hookTransform.rotation = fronthand.rotation;
            }
        }

        // update variables
        lastLeftHandPosition = LeftHand.position;
        lastRightHandPosition = RightHand.position;
        lastLeftHandRotation = LeftHand.rotation;
        lastRightHandRotation = RightHand.rotation;
    }

    void Move (Vector3 moveDirection)
    {
        if (!isObjectMode)
        {
            playerController.Move(moveDirection);
        }
        else
        {
            hookTransform.position += moveDirection;
        }
    }

    public void GenerateMeshCollider()
    {
        MeshCollider collider = GetComponent<MeshCollider>();

        if (collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }

        Mesh mesh = collider.sharedMesh;
        Line.BakeMesh(mesh, true);
        List<Vector3> vertices = new List<Vector3>();
        mesh.GetVertices(vertices);
        vertices.Add(PlayerAttachPoint.position - new Vector3(0,0.001f,0));

        collider.sharedMesh = mesh;
    }

    private void setLinePositionsWithHands ()
    {
        if (isLeftAttached && isRightAttached)
        {
            Line.positionCount = 4;
            // order attachment points based on distance to the player
            if (isLeftAttachmentNearPlayer)
            {
                Line.SetPositions(new Vector3[] {PlayerAttachPoint.position, LeftHand.position, RightHand.position, hookTransform.position});
            }
            else
            {
                Line.SetPositions(new Vector3[] {PlayerAttachPoint.position, RightHand.position, LeftHand.position, hookTransform.position});
            }
        } 
        else if (!isLeftAttached && isRightAttached)
        {
            Line.positionCount = 3;
            Line.SetPositions(new Vector3[] {PlayerAttachPoint.position, RightHand.position, hookTransform.position});
        }
        else if (isLeftAttached && !isRightAttached)
        {
            Line.positionCount = 3;
            Line.SetPositions(new Vector3[] {PlayerAttachPoint.position, LeftHand.position, hookTransform.position});
        }
        else if (!isLeftAttached && !isRightAttached)
        {
            Line.positionCount = 2;
            Line.SetPositions(new Vector3[] {PlayerAttachPoint.position, hookTransform.position});
        }
    }

    private void calculateAttachmentOrder()
    {
        float leftDistToPlayer = Vector3.Distance(PlayerAttachPoint.position, LeftHand.position);
        float rightDistToPlayer = Vector3.Distance(PlayerAttachPoint.position, RightHand.position);
        isLeftAttachmentNearPlayer = leftDistToPlayer < rightDistToPlayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        // layer 9 is Hands
        if (other.gameObject.layer == 9)
        {
            if (other.gameObject.tag == "left")
            {
                timeSinceLeftCollided = 0f;
            }
            else if (other.gameObject.tag == "right")
            {
                timeSinceRightCollided = 0f;
            }
        }
    }

    private bool isLeftHandCollidingWithRope ()
    {
        return timeSinceLeftCollided < 0.05f;
    }

    private bool isRightHandCollidingWithRope ()
    {
        return timeSinceRightCollided < 0.05f;
    }

    public void OnLeftGrab (InputAction.CallbackContext context)
    {
        if (!grapplingHook.isLeftHandCollidingWithHook && isLeftHandCollidingWithRope())
        {
            isLeftAttached = true;
            calculateAttachmentOrder();
            firstLeftAttachmentPosition = LeftHand.position;

            if (!isRightAttached || isRightAttached && isLeftAttachmentNearPlayer)
            {
                currentRadius = Vector3.Distance(LeftHand.position, hookTransform.position);
            }
        }
    }

    public void OnLeftRelease (InputAction.CallbackContext context)
    {
        isLeftAttached = false;
    }

    public void OnRightGrab (InputAction.CallbackContext context)
    {
        if (!grapplingHook.isRightHandCollidingWithHook && isRightHandCollidingWithRope())
        {
            //characterController.Move(new Vector3(10,0,0));
            isRightAttached = true;
            calculateAttachmentOrder();
            firstRightAttachmentPosition = RightHand.position;
            if (!isLeftAttached || isLeftAttached && !isLeftAttachmentNearPlayer)
            {
                currentRadius = Vector3.Distance(RightHand.position, hookTransform.position);
            }
        }
    }

    public void OnRightRelease (InputAction.CallbackContext context)
    {
        isRightAttached = false;
    }

    public void OnRightTrigger (InputAction.CallbackContext context)
    {
        if (isObjectMode && grapplingHook.isSomethingAttached())
        {
            grapplingHook.detachEverything();
            return;
        }
        isObjectMode = !isObjectMode;
        foreach (GameObject go in objectmodeIndicatorGroup)
        {
            go.SetActive(isObjectMode);
        }
        grapplingHook.switchToObjectMode(isObjectMode);
        Debug.Log("isObjectMode: " + isObjectMode);
        if(isRigidMode)
        {
            grapplingHook.rb.velocity = Vector3.zero;
        }
    }

    public void OnLeftTrigger (InputAction.CallbackContext context)
    {
       isRigidMode = !isRigidMode;
       Line.material = isRigidMode ? rigidMaterial : nonRigidMaterial;
       grapplingHook.switchToRigidMode(isRigidMode);
       Debug.Log("isRigidMode: " + isRigidMode);
       grapplingHook.rb.velocity = Vector3.zero;
    }

    public void OnOptionTrigger (InputAction.CallbackContext context)
    {
        OptionsMenu.SetActive(!OptionsMenu.activeSelf);
    }

    public void OnEnable()
    {
        leftGrabAction.Enable();
        leftReleaseAction.Enable();
        rightGrabAction.Enable();
        rightReleaseAction.Enable();
        leftTrigger.Enable();
        rightTrigger.Enable();
        optionButton.Enable();

        leftGrabAction.performed += OnLeftGrab;
        leftReleaseAction.performed += OnLeftRelease;
        rightGrabAction.performed += OnRightGrab;
        rightReleaseAction.performed += OnRightRelease;
        leftTrigger.performed += OnLeftTrigger;
        rightTrigger.performed += OnRightTrigger;
        optionButton.performed += OnOptionTrigger;
    }

    public void OnDisable()
    {
        leftGrabAction.performed -= OnLeftGrab;
        leftReleaseAction.performed -= OnLeftRelease;
        rightGrabAction.performed -= OnRightGrab;
        rightReleaseAction.performed -= OnRightRelease;
        leftTrigger.performed -= OnLeftTrigger;
        rightTrigger.performed -= OnRightTrigger;
        optionButton.performed -= OnOptionTrigger;

        leftGrabAction.Disable();
        leftReleaseAction.Disable();
        rightGrabAction.Disable();
        rightReleaseAction.Disable();
        leftTrigger.Disable();
        rightTrigger.Disable();
        optionButton.Disable();
    }
}
