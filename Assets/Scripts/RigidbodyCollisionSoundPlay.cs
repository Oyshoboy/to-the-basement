using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyCollisionSoundPlay : MonoBehaviour
{
    public SoundController soundController;
    
    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 17)
        {
            return;
        }
        soundController.PlayBodyObjectHitSound();
    }
}
