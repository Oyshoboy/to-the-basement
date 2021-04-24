using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollActivator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Player")
        {
            var playerRootManager = other.GetComponent<PlayerRootManager>();
            if (playerRootManager)
            {
                if (!playerRootManager.isRagdollToggled)
                {
                    playerRootManager.ToggleRagdoll();
                    playerRootManager.isRagdollToggled = true;
                }
            }
        }
    }
}
