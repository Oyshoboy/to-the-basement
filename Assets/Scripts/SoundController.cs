using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public AudioSource fxSoundsSource;
    public AudioSource fxWindSource;
    [SerializeField]
    private float velocityRatio;
    [SerializeField]
    private float windDefaultVolume;
    [Header("Hatch Sounds")]
    public AudioClip[] fxHatchCollisions;
    
    [Header("Coin Pickup Sounds")]
    public AudioClip[] fxCoinPickupSounds;
    public AudioClip[] fxCoinsBagSounds;
    
    [Header("NPC Collide Sounds")]
    public AudioClip[] npcGruntSounds;
    public AudioClip[] npcPowerUpSounds;
    public AudioClip[] npcHitSounds;
    public AudioClip[] breakSounds;
    public AudioClip[] pistonSounds;
    public AudioClip gameOverSound;
    public AudioClip barkSound;
    public AudioClip levelUpSound;
    public int maxCollisionSounds = 5;
    public int currentCollisionSoundsPlaying = 0;

    public float fxFragmentsDelay = 0.1f;
    public GameArcadeManager arcadeManager;
    public float velocitySoundModificator = 2f;
    
    private void Start()
    {
        windDefaultVolume = fxWindSource.volume;
    }

    public void PlayHatchCollisionSound(float volumeModificator)
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        var randomCollisionSound = fxHatchCollisions[UnityEngine.Random.Range(0, fxHatchCollisions.Length)];
        fxSoundsSource.PlayOneShot(randomCollisionSound, volumeModificator);
    }

    public void PlayCoinCollectionSound()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
        var randomSound = fxCoinPickupSounds[UnityEngine.Random.Range(0, fxCoinPickupSounds.Length)];
        fxSoundsSource.PlayOneShot(randomSound, .8f);
        var randomSound2 = fxCoinsBagSounds[UnityEngine.Random.Range(0, fxCoinsBagSounds.Length)];
        StartCoroutine(CoPlayDelayedClip(randomSound2, .4f, 2f));
    }

    public void PlayNPCCollisionSOund()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        var randomSound = npcHitSounds[UnityEngine.Random.Range(0, npcHitSounds.Length)];
        fxSoundsSource.PlayOneShot(randomSound);
        
        var randomSound3 = breakSounds[UnityEngine.Random.Range(0, breakSounds.Length)];
        fxSoundsSource.PlayOneShot(randomSound3, UnityEngine.Random.Range(0.3f, 0.8f));
        
        var randomSound2 = npcPowerUpSounds[UnityEngine.Random.Range(0, npcPowerUpSounds.Length)];
        StartCoroutine(CoPlayDelayedClip(randomSound2, .1f, 0.2f));
    }

    public void PlayBodyObjectHitSound()
    {
        if(currentCollisionSoundsPlaying >= maxCollisionSounds) return;
        
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        var randomSound = npcHitSounds[UnityEngine.Random.Range(0, npcHitSounds.Length)];
        fxSoundsSource.PlayOneShot(randomSound, 0.2f);
        
        currentCollisionSoundsPlaying++;
        StartCoroutine(CollisionSoundDelayedCleaner());
    }
    
    public void PlayNPCCollisionSOundSimple()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        var randomSound = npcHitSounds[UnityEngine.Random.Range(0, npcHitSounds.Length)];
        fxSoundsSource.PlayOneShot(randomSound);
    }
    
    public void PlayGameOverSound()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        fxSoundsSource.PlayOneShot(gameOverSound, .2f);
    }
    
    public void PlayPistonSound()
    {
        if (arcadeManager.gameManager.gameControls == GameManager.GameControlls.Stairs)
        {
            fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            fxSoundsSource.PlayOneShot(barkSound, 1.5f);
        }
        else
        {
            fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            var randomSound = pistonSounds[UnityEngine.Random.Range(0, pistonSounds.Length)];
            fxSoundsSource.PlayOneShot(randomSound, .35f);
        }
    }

    public void WindSoundController()
    {
        velocityRatio = arcadeManager.gameManager.playerVelocityLimiter.currentVelocity / ( arcadeManager.levelCalculatedParams.maxVelocity / velocitySoundModificator );
        velocityRatio = velocityRatio >= 1f ? 1f : velocityRatio;
        velocityRatio = velocityRatio <= 0.05f ? 0.05f : velocityRatio;

        fxWindSource.volume = windDefaultVolume * velocityRatio;
    }

    public void PlayBarkingSound()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        fxSoundsSource.PlayOneShot(barkSound, .5f);
    }
    
    public void PlayLevelupSound()
    {
        fxSoundsSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
        fxSoundsSource.PlayOneShot(levelUpSound, .5f);
    }

    private void Update()
    {
        WindSoundController();
    }

    IEnumerator CollisionSoundDelayedCleaner() {
        yield return new WaitForSeconds(.3f);
        currentCollisionSoundsPlaying--;
    }

    IEnumerator CoPlayDelayedClip(AudioClip audioClip, float delay, float volume) {
        yield return new WaitForSeconds(delay);
        fxSoundsSource.PlayOneShot(audioClip, volume);
    }
}
