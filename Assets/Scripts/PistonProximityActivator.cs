using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonProximityActivator : MonoBehaviour
{
    public GameObject playerObject;
    [SerializeField] private PistonController[] pistons;
    [SerializeField] private int closestPiston = 0;
    [SerializeField] private int previousPiston = 0;
    private bool isplayerObjectNotNull;
    [SerializeField] private bool isPistonButtonReleaseNeeded = false;
    [SerializeField] private bool isLastPistonActivated;
    [SerializeField] private GameManager gameManager;
    
    void Start()
    {
        InitRefferences();
    }

    void Update()
    {
        if (isplayerObjectNotNull && pistons.Length > 0)
        {
            if (gameManager.gameState != GameManager.GameState.Beginning)
            {
                pistonsProximityController();
            }
        }
        else
        {
            InitRefferences();
        }
    }

    private void InitRefferences()
    {
        isplayerObjectNotNull = playerObject != null;
        playerObject = GameObject.FindWithTag("Player");
        pistons = GameObject.FindObjectsOfType<PistonController>();
    }

    private void pistonsProximityController()
    {
        float minDistance = float.MaxValue;

        for (int i = 0; i < pistons.Length; i++)
        {
            pistons[i].isPistonActive = false;
            var currentDistance = Vector3.Distance(playerObject.transform.position, pistons[i].transform.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                
                // ONLY IF LAST PISTON WASN'T ACTIVATED
                if (!isLastPistonActivated)
                {
                    closestPiston = i;
                }
            }
        }

        // DETECT IF PISTONS WAS SWITCHED
        if (previousPiston != closestPiston)
        {
            previousPiston = closestPiston;
            isPistonButtonReleaseNeeded = true;
        }

        // CHECK IF BACKSPACE NEED TO BE RELEASED, TO AVOID AUTOMATIC EXTENTION IF BACKSPACE IS PRESSED AND HELD
        if (pistons[closestPiston].isPistonExtending && !isPistonButtonReleaseNeeded)
        {
            // CHECK IF LAST PISTON WAS ACTIVATED
            if (pistons[closestPiston].isThisPistonLast && !isLastPistonActivated)
            {
                isLastPistonActivated = true;
            }
            
            // ACTIVATE NEAREST PISTON
            pistons[closestPiston].isPistonActive = true;
        }
        else if (isPistonButtonReleaseNeeded && !pistons[closestPiston].isPistonExtending)
        {
            isPistonButtonReleaseNeeded = false;
        }
    }
}