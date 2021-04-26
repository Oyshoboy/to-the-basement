using System.Collections;
using System.Collections.Generic;
using RayFire;
using UnityEngine;

public class RayfireIntitialize : MonoBehaviour
{
    public GameObject collisionCollider;
    public RayfireRigid rayfireRigid; 
    // Start is called before the first frame update
    void Start()
    {
        if (rayfireRigid != null)
        {
            rayfireRigid.demolitionEvent.LocalEvent += DisableCollisionColliderOnDemolish;
            rayfireRigid.GetComponent<RayfireRigid>();
            rayfireRigid.Initialize();
        }
    }

    void DisableCollisionColliderOnDemolish(RayfireRigid rigid)
    {
        //collisionCollider.SetActive(false);
        Destroy(collisionCollider, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
