using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class PlayerRootManager : MonoBehaviour
{
    public Animator playerAnimator;
    public PuppetMaster playerPuppetMaster;
    public Rigidbody playerRigbody;
    public SphereCollider playerCollider;

    public bool isRagdollToggled;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ToggleRagdoll()
    {
        playerAnimator.enabled = false;
        playerCollider.enabled = false;
        playerRigbody.isKinematic = true;
        playerPuppetMaster.state = PuppetMaster.State.Dead;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
