using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonusEffectHandler : MonoBehaviour
{
    public GameObject bonusParticles;
    public GameObject pickupParticles;

    public void activateParticles()
    {
        pickupParticles.SetActive(true);
    }
}
