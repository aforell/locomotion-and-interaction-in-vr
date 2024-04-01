using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocomotionTechnique : MonoBehaviour
{
    // Please implement your locomotion technique in this script. 
    public GameObject hmd;

    /////////////////////////////////////////////////////////
    // These are for the game mechanism.
    public ParkourCounter parkourCounter;
    public string stage;
    public SelectionTaskMeasure selectionTaskMeasure;
    public InputAction tpToStartAction;
    public GrapplingHook grapplingHook;
    
    void Start()
    {
        
    }

    void Update()
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        // Please implement your LOCOMOTION TECHNIQUE in this script :D.
    }

    public void tpToStart (InputAction.CallbackContext context)
    {
        ////////////////////////////////////////////////////////////////////////////////
        // These are for the game mechanism.
        Debug.Log("tpButton: " + Time.time);
        if (parkourCounter.parkourStart)
        {
            this.transform.position = parkourCounter.currentRespawnPos;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Enter: " + Time.time);
        // These are for the game mechanism.
        if (other.CompareTag("banner"))
        {
            stage = other.gameObject.name;
            parkourCounter.isStageChange = true;
        }
        else if (other.CompareTag("objectInteractionTask"))
        {
            selectionTaskMeasure.isTaskStart = true;
            selectionTaskMeasure.scoreText.text = "";
            selectionTaskMeasure.partSumErr = 0f;
            selectionTaskMeasure.partSumTime = 0f;
            // rotation: facing the user's entering direction
            float tempValueY = other.transform.position.y > 0 ? 12 : 0;
            Vector3 tmpTarget = new Vector3(hmd.transform.position.x, tempValueY, hmd.transform.position.z);
            selectionTaskMeasure.taskUI.transform.LookAt(tmpTarget);
            selectionTaskMeasure.taskUI.transform.Rotate(new Vector3(0, 180f, 0));
            selectionTaskMeasure.taskStartPanel.SetActive(true);
        }
        else if (other.CompareTag("coin"))
        {
            CollectCoin(other.gameObject);
        }
        // These are for the game mechanism.
    }

    public void CollectCoin (GameObject coin)
    {
        parkourCounter.coinCount += 1;
        this.GetComponent<AudioSource>().Play();
        coin.GetComponent<MovableObject>().detach(grapplingHook);
        grapplingHook.attachedMovables.Remove(coin.GetComponent<MovableObject>());
        coin.SetActive(false);
    }

    public void OnEnable()
    {
        tpToStartAction.Enable();

        tpToStartAction.performed += tpToStart;
    }

    public void OnDisable()
    {
        tpToStartAction.performed -= tpToStart;

        tpToStartAction.Disable();
    }
}