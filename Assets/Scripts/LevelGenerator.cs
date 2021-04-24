using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class LevelGenerator : MonoBehaviour
{
    [Header("Generator config")]
    [SerializeField] private int initialPoolSize = 5;
    [SerializeField] private float chunkHeight = 12f;
    private float lowestStaticLevelOffset;
    
    [SerializeField] private List<GameObject> tunnelChunksPool;
    [SerializeField] private Vector3 sceneStartPosition;

    [SerializeField] private float lastChunkIterationPlaced = 1;
    [SerializeField] private float heightDifferenceSingle;

    public GameObject[] tunnelChunksSpawnObjects;
    public GameObject defaultChunkHide;

    private int chunksSwitchIndes = 0;
    // Start is called before the first frame update
    void Start()
    {
        InitBaseTunnelChunks();
    }

    public void InitBaseTunnelChunks()
    {
        defaultChunkHide.SetActive(false);
        for (int i = 0; i < initialPoolSize; i++)
        {
            var objectSpawned = Instantiate(tunnelChunksSpawnObjects[UnityEngine.Random.Range(0, tunnelChunksSpawnObjects.Length)]);
            objectSpawned.transform.parent = defaultChunkHide.transform.parent;
            objectSpawned.transform.localPosition = new Vector3(0, -( chunkHeight * i ), 0);
            tunnelChunksPool.Add(objectSpawned);
        }
        sceneStartPosition = transform.position;
        lowestStaticLevelOffset -= ( tunnelChunksPool.Count - 1 ) * chunkHeight;
    }

    private void TunnelChunksDepthPlacement()
    {
        var heightDifference = (transform.position.y - sceneStartPosition.y) / chunkHeight;
        heightDifferenceSingle = Mathf.Abs(Mathf.Floor(heightDifference));

        if (heightDifferenceSingle > lastChunkIterationPlaced)
        {
            Debug.Log("REPLACE CHUNKS!");
            lastChunkIterationPlaced = heightDifferenceSingle;
            lowestStaticLevelOffset -= chunkHeight;
            tunnelChunksPool[chunksSwitchIndes].transform.position = new Vector3(tunnelChunksPool[chunksSwitchIndes].transform.position.x, lowestStaticLevelOffset, tunnelChunksPool[chunksSwitchIndes].transform.position.z);
            chunksSwitchIndes++;

            // RESET CHUNKS INDEX
            if (chunksSwitchIndes == tunnelChunksPool.Count)
            {
                chunksSwitchIndes = 0;
            }
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        TunnelChunksDepthPlacement();
    }
}
