using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteMusic : MonoBehaviour
{
    public void switchMuteState()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.mute = !audio.mute;
    }
}
