using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpButtonController : MonoBehaviour
{
    public PlayerStatsController statsController;
    private void OnMouseDown()
    {
        statsController.LevelUp();
    }
}
