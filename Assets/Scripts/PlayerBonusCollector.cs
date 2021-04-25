using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBonusCollector : MonoBehaviour
{
    public int moneyCollected = 0;
    public int npcCollided = 0;
    public Transform playerHips;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);
        if (other.name == "Floater")
        {
            moneyCollected++;
        }

        if (other.name == "Player_Nepic")
        {
            other.gameObject.layer = 17;
            npcCollided++;
        }
    }

    private void Update()
    {
        transform.localPosition = playerHips.localPosition;
    }
}
