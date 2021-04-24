using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelStartTrigger : MonoBehaviour
{
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            gameManager.MoveCameraToFallingMode();
            gameManager.currentPlayerAnimator.SetTrigger("Falling");
        }
    }
}
