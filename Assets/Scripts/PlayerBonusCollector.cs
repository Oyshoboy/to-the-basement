using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerBonusCollector : MonoBehaviour
{
    public GameArcadeManager gameArcadeManager;
    public int moneyCollected = 0;
    public int npcCollided = 0;
    public Transform playerHips;
    public SoundController soundController;
    public float hatchCollisionVolumeModificator = 1f;
    
    private void IncreaseMoney()
    {
        var prevMoneySaved = PlayerPrefs.GetFloat("TotalEarned");
        moneyCollected++;
        PlayerPrefs.SetFloat("TotalEarned", prevMoneySaved + 1);
    }
    
    private void IncreaseNpcCollisions()
    {
        var prevMoneySaved = PlayerPrefs.GetFloat("NpcTotalCollided");
        npcCollided++;
        PlayerPrefs.SetFloat("NpcTotalCollided", prevMoneySaved + 1);
    }

    private void Start()
    {
        var prevMoneySaved = PlayerPrefs.GetFloat("TotalEarned");
        //Debug.Log($"Total money earned: {prevMoneySaved}");
        
        var totalNpcCollided = PlayerPrefs.GetFloat("NpcTotalCollided");
        //Debug.Log($"Total NPC collided: {totalNpcCollided}");
        
        var maxDistanceTraveledRecord = PlayerPrefs.GetFloat("MaxDistanceTraveled");
        //Debug.Log($"Distance record: {maxDistanceTraveledRecord}");
        
        var playerSkillLevel = PlayerPrefs.GetFloat("PlayerSkillLevel");
        //Debug.Log($"Player skill level: {playerSkillLevel + 1}");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "HatchCollider")
        {
            soundController.PlayHatchCollisionSound(hatchCollisionVolumeModificator);
        }
        
        if (other.name == "Floater")
        {
            other.gameObject.layer = 17;
            var otherEffectHandler = other.transform.parent.GetComponent<BonusEffectHandler>();
            otherEffectHandler.bonusParticles.SetActive(false);
            IncreaseMoney();
            otherEffectHandler.pickupParticles.SetActive(true);
            soundController.PlayCoinCollectionSound();
        }

        if (other.name == "Player_Nepic")
        {
            other.gameObject.layer = 17;
            IncreaseNpcCollisions();
            gameArcadeManager.AddGase();
            soundController.PlayNPCCollisionSOundSimple();
        }
    }

    private void Update()
    {
        transform.localPosition = playerHips.localPosition;
    }
}