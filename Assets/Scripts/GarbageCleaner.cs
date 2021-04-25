using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageCleaner : MonoBehaviour
{
   public LevelObjectsSpawner levelObjectsSpawner;
   private void OnTriggerEnter(Collider other)
   {
      if (other.name == "Player_Nepic" || other.name == "Floater")
      {
         Destroy(other.transform.parent.gameObject);
         levelObjectsSpawner.NPCSpawned--;
      }
   }
}
