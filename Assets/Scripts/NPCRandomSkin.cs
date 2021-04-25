using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class NPCRandomSkin : MonoBehaviour
{
    public GameObject[] skins;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].SetActive(false);
        }
        
        skins[UnityEngine.Random.Range(0, skins.Length -1)].SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
