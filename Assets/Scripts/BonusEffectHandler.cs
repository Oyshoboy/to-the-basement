using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusEffectHandler : MonoBehaviour
{
    public GameObject bonusParticles;
    public GameObject pickupParticles;
    public GameManager gameManager;
    public void activateParticles()
    {
        pickupParticles.SetActive(true);
        gameManager.arcadeManager.bonusCollector.soundController.PlayNPCCollisionSOund();
    }

    private void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameManager>();
    }
}
