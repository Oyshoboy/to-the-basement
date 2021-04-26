using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class GameControlsToggler : MonoBehaviour
{
    public GameObject gameControlsObject;
    public GameObject playerMenuObject;

    private void Update()
    {
        //.Log($"Times played: {PlayerPrefs.GetFloat("TimesPlayed")}");
        if (PlayerPrefs.GetFloat("TimesPlayed") > 0)
        {
            gameControlsObject.SetActive(false);
            //playerMenuObject.SetActive(true);
        }
        else
        {
            gameControlsObject.SetActive(true);
           // playerMenuObject.SetActive(false);
        }
    }
}
