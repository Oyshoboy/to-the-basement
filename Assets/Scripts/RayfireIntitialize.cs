using System.Collections;
using System.Collections.Generic;
using RayFire;
using UnityEngine;

public class RayfireIntitialize : MonoBehaviour
{
    public RayfireRigid rayfireRigid; 
    // Start is called before the first frame update
    void Start()
    {
        if (rayfireRigid != null)
        {
            rayfireRigid.GetComponent<RayfireRigid>();
            rayfireRigid.Initialize();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
