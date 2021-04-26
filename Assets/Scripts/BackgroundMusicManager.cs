using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusicManager : MonoBehaviour
{
    private static BackgroundMusicManager backgroundMusicManagerInstance;
    public AudioSource backGroundMusicSource;
    public AudioClip[] backgroundMusicSamples;
    public float[] backgroundMusicVolumes;
    public int currentClipIndexPlaying = 0;
    public GameManager gameManager;
    public float transitionTime = 1f;
    public float transitionElapsedTime = 0f;
    private bool isMusicTransitionRequested = false;
    private float musicDefaultVolume = 1f;
    private float musicDefaultPitch = 1f;
    public float[] musicStatePitches;
    private bool isSoundFadingOut = false;
    private float velocityRatio = 0f;
    public bool isSceneSwitched = false;

    public enum SoundTransitionType
    {
        Switch,
        Velocity,
        VelocitySwitch
    }

    public SoundTransitionType switchType;
    
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
        musicDefaultPitch = backGroundMusicSource.pitch;
        backGroundMusicSource.volume = musicDefaultVolume * backgroundMusicVolumes[0];
    }

    private void MusicStateController()
    {
        if (gameManager)
        {
            if(!gameManager.arcadeManager) return;

            if (isSceneSwitched)
            {
                if (currentClipIndexPlaying != 3 && gameManager.gameState != GameManager.GameState.ToTheDepth)
                {
                    RequestNextClip(3);
                } else if (currentClipIndexPlaying == 3 && gameManager.gameState == GameManager.GameState.ToTheDepth)
                {
                    RequestNextClip(4);
                }
            } else if (gameManager.arcadeManager.isGameOver)
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
        backGroundMusicSource.volume = Mathf.Lerp(0, musicDefaultVolume * backgroundMusicVolumes[currentClipIndexPlaying], smoothStep);
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
                Debug.Log("Silence");
                SwitchClipAndResetTransitionElapsedTime();
                //backGroundMusicSource.volume = musicDefaultVolume * backgroundMusicVolumes[currentClipIndexPlaying];
            }
        }
    }

    void MusicPitchVelocityTransitionHandler()
    {
        velocityRatio = gameManager.playerVelocityLimiter.currentVelocity / ( gameManager.arcadeManager.levelCalculatedParams.maxVelocity / 5 );
        velocityRatio = velocityRatio >= 1f ? 1f : velocityRatio;
        velocityRatio = velocityRatio <= 0.33f ? 0.33f : velocityRatio;

        backGroundMusicSource.pitch = musicDefaultPitch * velocityRatio;
    }
    
    void MusicVelocitySwitchTransitionType()
    {
        velocityRatio = gameManager.playerVelocityLimiter.currentVelocity / ( gameManager.arcadeManager.levelCalculatedParams.maxVelocity / 5 );
        velocityRatio = velocityRatio >= 1f ? 1f : velocityRatio;
        velocityRatio = velocityRatio <= 0.33f ? 0.33f : velocityRatio;

        backGroundMusicSource.pitch = musicDefaultPitch * velocityRatio;
    }

    private void Update()
    {
        MusicStateController();

        if (SceneManager.GetActiveScene().name == "Development")
        {
            isSceneSwitched = true;
        }
        else
        {
            isSceneSwitched = false;
        }
        
        if (switchType == SoundTransitionType.Switch)
        {
            MusicTransitionHandler();            
        } else if (switchType == SoundTransitionType.Velocity)
        {
            MusicPitchVelocityTransitionHandler();
        } else if (switchType == SoundTransitionType.VelocitySwitch)
        {
            MusicVelocitySwitchTransitionType();
        }
    }
}