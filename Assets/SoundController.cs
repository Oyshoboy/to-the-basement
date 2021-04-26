using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource fxSoundsSource;
    public AudioClip[] fxAudioClips;
    private void Start()
    {
        //fxSoundsSource.clip = fxAudioClips[0];
        //fxSoundsSource.Play();
    }
}
