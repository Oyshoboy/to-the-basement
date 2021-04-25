using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelStartTrigger : MonoBehaviour
{
    public GameManager gameManager;
    public UnityEvent triggeredEvent;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            triggeredEvent.Invoke();
            //gameManager.MoveCameraToFallingMode();
            //gameManager.currentPlayerAnimator.SetTrigger("Falling");
        }
    }
}
