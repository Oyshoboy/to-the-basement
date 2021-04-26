using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager backgroundMusicManagerInstance;
    public AudioSource backGroundMusicSource;
    public AudioClip[] backgroundMusicSamples;
    public int currentClipIndexPlaying = 0;
    public GameManager gameManager;
    public float transitionTime = 1f;
    public float transitionElapsedTime = 0f;
    private bool isMusicTransitionRequested = false;
    private float musicDefaultVolume = 1f;
    private bool isSoundFadingOut = false;
    
    private void Awake()
    {
        if (backgroundMusicManagerInstance == null)
        {
            backgroundMusicManagerInstance = this;
            DontDestroyOnLoad(backgroundMusicManagerInstance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        backGroundMusicSource.clip = backgroundMusicSamples[0];
        backGroundMusicSource.Play();
        musicDefaultVolume = backGroundMusicSource.volume;
    }

    private void MusicStateController()
    {
        if (gameManager)
        {
            if(!gameManager.arcadeManager) return;
            
            if (gameManager.arcadeManager.isGameOver)
            {
                if (currentClipIndexPlaying == 1)
                {
                    RequestNextClip(2);
                    Debug.Log("Playing gameover music");
                } else if (gameManager.isRestartRequested && currentClipIndexPlaying == 2)
                {
                    RequestNextClip(0);
                    Debug.Log("Playing default on gameover restart requested");
                }
            } else if (gameManager.gameState == GameManager.GameState.Falling && currentClipIndexPlaying == 0 && !gameManager.isRestartRequested)
            {
                RequestNextClip(1);
                Debug.Log("Playing tension music");
            }
            else if (gameManager.gameState != GameManager.GameState.Falling && currentClipIndexPlaying != 0)
            {
                RequestNextClip(0);
                Debug.Log("Playing default music on scene reload");
            }else if (gameManager.isRestartRequested && currentClipIndexPlaying == 1)
            {
                RequestNextClip(0);
                Debug.Log("Playing default music on restart requested");
            }
        }
    }

    private void RequestNextClip(int clipIndex)
    {
        currentClipIndexPlaying = clipIndex;
        isMusicTransitionRequested = true;
    }

    void FadeOutMusic()
    {
        var firstHalfTimeElapse = transitionTime / 2;
        var percent = transitionElapsedTime / transitionTime;
        transitionElapsedTime += Time.deltaTime / transitionTime;
        var smoothStep = Mathf.SmoothStep(0, 1, percent);
        backGroundMusicSource.volume = Mathf.Lerp(musicDefaultVolume, 0, smoothStep);
    }

    void FadeInMusic()
    {
        var firstHalfTimeElapse = transitionTime / 2;
        var percent = transitionElapsedTime / transitionTime;
        transitionElapsedTime += Time.deltaTime / transitionTime;
        var smoothStep = Mathf.SmoothStep(0, 1, percent);
        backGroundMusicSource.volume = Mathf.Lerp(0, musicDefaultVolume, smoothStep);
    }

    private void SwitchClipAndResetTransitionElapsedTime()
    {
        backGroundMusicSource.clip = backgroundMusicSamples[currentClipIndexPlaying];
        transitionElapsedTime = 0;
        backGroundMusicSource.Play();
    }

    private void MusicTransitionHandler()
    {
        if (isMusicTransitionRequested && transitionElapsedTime < transitionTime)
        {
            if (isSoundFadingOut)
            {
                FadeInMusic();
            }
            else
            {
                FadeOutMusic();
            }
        } else if (isMusicTransitionRequested && transitionElapsedTime >= transitionTime)
        {
            if (isSoundFadingOut)
            {
                isSoundFadingOut = false;
                isMusicTransitionRequested = false;
            } else
            {
                isSoundFadingOut = true;
                SwitchClipAndResetTransitionElapsedTime();
            }
        }
    }

    private void Update()
    {
        MusicStateController();
        MusicTransitionHandler();
    }
}