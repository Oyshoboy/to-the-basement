using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class StairChunkObjectSpawner : MonoBehaviour
{
    public SpawnLine[] spawnLines;
    public GameObject[] objectsToSpawn;
    public LevelObjectsSpawner levelObjectsSpawner;
    private int spawnedLinesLastIndex = 0;
     
    private void Start()
    {
        levelObjectsSpawner = GameObject.FindGameObjectWithTag("GameController").GetComponent<LevelObjectsSpawner>();
        for (int i = 0; i < spawnLines.Length; i++)
        {
            var spawnPosition = spawnLines[i].spawnPoints[UnityEngine.Random.Range(0, spawnLines[i].spawnPoints.Length)];
            var objectToSpawn = objectsToSpawn[UnityEngine.Random.Range(0, objectsToSpawn.Length)];

            var spawnedObject = Instantiate(objectToSpawn, spawnPosition.position, Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0));
            spawnedObject.transform.parent = spawnPosition;
            spawnedObject.transform.localPosition = spawnPosition.transform.localPosition;

        }
    }

    private void Update()
    {
        
    }
}
