using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class NPCFallingController : MonoBehaviour
{
    public PuppetMaster myPuppetMaster;
    public Animator myAnimator;
    public string animationName;

    public void SwitchFallingState()
    {
        myAnimator.SetFloat(animationName, 1f);
    }
}
