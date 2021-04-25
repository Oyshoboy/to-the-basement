using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelObjectsSpawner : MonoBehaviour
{
    [Header("ObjectSpawnSettings")] public int maxNpcToSpawn = 10;
    public int NPCSpawned = 0;
    public LevelGenerator levelGenerator;

    public List<SpawnLine> queueToSpawn;
    public GameObject[] objectsToSpawn;

    public void SpawnUpcomingChunkObjects(SpawnLine spawnLine)
    {
        var spawnPosition = spawnLine.spawnPoints[UnityEngine.Random.Range(0, spawnLine.spawnPoints.Length)];
        var objectToSpawn = objectsToSpawn[UnityEngine.Random.Range(0, objectsToSpawn.Length)];

        var spawnedObject = Instantiate(objectToSpawn, spawnPosition.position,
            Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0));
        spawnedObject.transform.parent = spawnPosition;
        spawnedObject.transform.localPosition = spawnPosition.transform.localPosition;
        
        queueToSpawn.RemoveAt(0);
        NPCSpawned++;
    }

    private void FixedUpdate()
    {
        if (NPCSpawned < maxNpcToSpawn)
        {
            SpawnUpcomingChunkObjects(queueToSpawn[0]);
        }
    }
}