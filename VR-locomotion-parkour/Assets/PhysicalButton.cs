using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhysicalButton : MonoBehaviour
{
    public GameObject button;
    public UnityEvent onPress;
    public UnityEvent onRelease;
    private bool isPressed = false;
    public MeshRenderer meshRenderer;
    public ButtonState states = new ButtonState();
    private AudioSource sound;
    
    void Start ()
    {
        sound = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.003f, 0);
            onPress.Invoke();
            playsound(1.1f);
            isPressed = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed)
        {
            button.transform.localPosition = new Vector3(0, 0.015f, 0);
            onRelease.Invoke();
            isPressed = false;
            if (states.hasStates())
            {
                meshRenderer.material = states.getNextMaterial();
            }
            playsound(0.9f);
        }
    }

    private void playsound (float startPitch)
    {
        float lowPitch = startPitch - 0.1f * startPitch;
        float highPitch = startPitch + 0.1f * startPitch;
        sound.pitch = Random.Range(lowPitch, highPitch);
        sound.Play();
    }
}
