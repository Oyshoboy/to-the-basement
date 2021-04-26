using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomBarkingController : MonoBehaviour
{

    public SoundController soundController;

    private void Start()
    {
        StartCoroutine(RandomBarker());
    }

    IEnumerator RandomBarker()
    {
        if (Random.value > .35f)
        {
            if (soundController.arcadeManager.gameManager.gameState != GameManager.GameState.Falling)
            {
                soundController.PlayBarkingSound();
            }
        }
        var wait_time = Random.Range (0.1f, 5f);
        yield return new WaitForSeconds (wait_time);
        StartCoroutine(RandomBarker());
    }
}
