using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerStatsController : MonoBehaviour
{
  public GameArcadeManager arcadeManager;
  public TextMeshPro[] statsTextes;
  public GameObject levelUpButton;
  public float levelUpPrice = 5f;
  private float totalEarned = 0;
  private float currentLevel = 0;
  private float maxLevel = 0;
  public ParticleSystem levelUpParticles;
  private void LevelUpButtonAppearController()
  {
    if (totalEarned >= levelUpPrice && currentLevel < maxLevel)
    {
      levelUpButton.SetActive(true);
    }
    else
    {
      levelUpButton.SetActive(false);
    }
  }

  private void StatsDisplayController()
  {
    totalEarned = PlayerPrefs.GetFloat("TotalEarned");
    statsTextes[0].text = $"Wallet: {arcadeManager.FloatToThreeDigitText(totalEarned * arcadeManager.coinPrice)}$";

    currentLevel = PlayerPrefs.GetFloat("PlayerSkillLevel");
    maxLevel = arcadeManager.playerMaxSkills.level;
    statsTextes[1].text = $"Lv. {currentLevel + 1}/{maxLevel}";
    
    var maxDistanceTraveled = PlayerPrefs.GetFloat("MaxDistanceTraveled");
    statsTextes[2].text = $"Record {arcadeManager.FloatToThreeDigitText(maxDistanceTraveled)}m";
  }

  public void LevelUp()
  {
    if (arcadeManager.gameManager.gameState == GameManager.GameState.Beginning || arcadeManager.gameManager.gameState == GameManager.GameState.Start)
    {
      if (totalEarned >= levelUpPrice && currentLevel < maxLevel)
      {
        PlayerPrefs.SetFloat("TotalEarned", totalEarned - levelUpPrice);
        PlayerPrefs.SetFloat("PlayerSkillLevel", currentLevel + 1);
        Debug.Log("LEVEL UPPING!");
        levelUpParticles.Play();
      }
    }
  }

  private void FixedUpdate()
  {
    StatsDisplayController();
    LevelUpButtonAppearController();
  }
}
