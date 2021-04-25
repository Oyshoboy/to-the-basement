using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;

public class GameArcadeManager : MonoBehaviour
{
    [Header("Arcade Parameters")]
    public float gameSkillOverallModif = 1f;
    public float gameExhaustSpeed = 0.1f;
    public float gameExhaustByTime = 0.85f;
    
    public float playerControlsForce = 1000f;
    float playerVlocitySmoother = 0.95f;
    
    public float gasPushForcePower;
    public float playerTotalDistanceTraveled;
    
    [Header("Player skills")]
    public float playerMaxVelocity = 100f;
    public float playerGas = 100;
    public float mpgModificator = 1;
    public float fallingHelperPushForce = 1000f;
    public float wallsMaxSolidity = 0.55f;

    [Header("Refferences")] 
    public GameManager gameManager;
    public GameObject playerPhysicsRoot;
    public PlayerVelocityLimiter playerVelocityLimiter;
    public PuppetMaster playerPuppetMaster;
    
    //SYSTEM VARIABLES
    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField] private float maxDistanceTraveled;
    // Start is called before the first frame update
    void Start()
    {
        playerStartPosition = playerPhysicsRoot.transform.position;
        playerVlocitySmoother = playerVelocityLimiter.velocitySmoother;
        gameManager.playerMovementSpeed = playerControlsForce;
        gameManager.fallingHelperPushForce = fallingHelperPushForce;
    }

    private void TotalDistanceTraveledTracker()
    {
        playerTotalDistanceTraveled = Vector3.Distance(playerStartPosition, playerPhysicsRoot.transform.position);
        if (playerTotalDistanceTraveled > maxDistanceTraveled)
        {
            IncreaseGas(-((playerTotalDistanceTraveled - maxDistanceTraveled) * mpgModificator));
            maxDistanceTraveled = playerTotalDistanceTraveled;
        }
    }

    private void IncreaseGas(float decreaseFactor)
    {
        playerGas += decreaseFactor;
    }

    private void PlayerOverallModifController()
    {
        if (gameManager.gameState == GameManager.GameState.Falling)
        {
            playerGas -= Time.deltaTime * gameExhaustByTime;
        }

        if (playerGas <= 0)
        {
            gameSkillOverallModif -= Time.deltaTime * gameExhaustSpeed;
            
            if (gameSkillOverallModif <= 0)
            {
                gameSkillOverallModif = 0;
            }
            
            PlayerExhaust();
            playerGas = 0;
        }
    }


    private void PlayerExhaust()
    {
        var velocityDecreaseFactor = ( Time.deltaTime * gameExhaustSpeed ) * 100;
        playerVelocityLimiter.pelvisMaxVelocity -= velocityDecreaseFactor;
        playerVelocityLimiter.restMaxVelocity -= velocityDecreaseFactor / 2;
        
        
        gameManager.playerMovementSpeed -= playerControlsForce * gameSkillOverallModif;
        gameManager.fallingHelperPushForce -= fallingHelperPushForce * gameSkillOverallModif;

        if (gameManager.playerMovementSpeed <= 0)
        {
            gameManager.playerMovementSpeed = 0;
        }

        if (gameManager.fallingHelperPushForce <= 0)
        {
            gameManager.fallingHelperPushForce = 0;
        }
        
        if (playerVelocityLimiter.pelvisMaxVelocity <= 0)
        {
            playerVelocityLimiter.pelvisMaxVelocity = 0;
            playerVelocityLimiter.velocitySmoother = playerVlocitySmoother * 0.95f;
            playerPuppetMaster.state = PuppetMaster.State.Dead;
        }
        
        if (playerVelocityLimiter.restMaxVelocity <= 0)
        {
            playerVelocityLimiter.restMaxVelocity = 0;
            playerVelocityLimiter.restVelocitySmoother = playerVlocitySmoother * 0.95f;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TotalDistanceTraveledTracker();
        PlayerOverallModifController();
    }
}
