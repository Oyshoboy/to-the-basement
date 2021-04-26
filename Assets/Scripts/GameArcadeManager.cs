using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEditor.Build.Player;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerSkills
{
    public float level = 1;
    public float maxVelocity = 300f;
    public float mpg = 1f;
    public float mpt = 1f;
    public float fallingPushForce = 200f;
    public float controlForce = 1000f;
    public float npcGasPrice = 5f;
    public float wallsSolidity = 0.3f;
}

public class GameArcadeManager : MonoBehaviour
{
    [Header("UI Stuff")] public TextMesh[] uiTexts;

    [Header("Economics")]
    public int gasPerNPCCollision = 10;
    public int coinPrice = 100;

    [Header("Arcade Parameters")]
    public float gameSkillOverallModif = 1f;
    public float gameExhaustSpeed = 0.1f;
    public float gameExhaustByTime = 0.85f;
    float playerVlocitySmoother = 0.93f;
    public float playerGas = 100;

    public float gasPushForcePower;
    public float playerTotalDistanceTraveled;


    [Header("Player skills")]
    public PlayerSkills playerMaxSkills;
    public PlayerSkills playerDefaultSkills;
    public PlayerSkills levelCalculatedParams;

    [Header("Refferences")] 
    public GameManager gameManager;
    public PlayerBonusCollector bonusCollector;
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
        gameManager.playerMovementSpeed = levelCalculatedParams.controlForce;
        gameManager.fallingHelperPushForce = levelCalculatedParams.fallingPushForce;
    }

    public void SetupLevelConfigurationBasedOnPlayerLevel()
    {
        
    }
    
    // SKILLS CALCULATORS
    public float GasMptCalculator()
    {
        return levelCalculatedParams.mpt;
    }

    private void TotalDistanceTraveledTracker()
    {
        playerTotalDistanceTraveled = Vector3.Distance(playerStartPosition, playerPhysicsRoot.transform.position);
        if (playerTotalDistanceTraveled > maxDistanceTraveled)
        {
            IncreaseGas(-((playerTotalDistanceTraveled - maxDistanceTraveled) * levelCalculatedParams.mpg));
            maxDistanceTraveled = playerTotalDistanceTraveled;
            PlayerPrefs.SetFloat("MaxDistanceTraveled", maxDistanceTraveled);
        }
    }

    public void AddGase()
    {
        playerGas += levelCalculatedParams.npcGasPrice;
    }

    private string FloatToThreeDigitText(float value)
    {
        if (value < 100 && value > 9)
        {
            return $"0{Mathf.Floor(value)}";
        } else if (value < 100 && value < 10)
        {
            return $"00{Mathf.Floor(value)}";
        }
        else
        {
            return $"{Mathf.Floor(value)}";
        }
    }

    private void UIDynamicUpdate()
    {
        uiTexts[0].text = $"x {FloatToThreeDigitText (bonusCollector.moneyCollected * coinPrice )}";
        uiTexts[1].text = $"{FloatToThreeDigitText (playerVelocityLimiter.currentVelocity)}km/h";
        uiTexts[2].text = $"x {FloatToThreeDigitText (bonusCollector.npcCollided)}";
        uiTexts[3].text = $"{FloatToThreeDigitText (maxDistanceTraveled)}m";
    }

    private void IncreaseGas(float decreaseFactor)
    {
        playerGas += decreaseFactor;
    }

    private void PlayerOverallModifController()
    {
        if (gameManager.gameState == GameManager.GameState.Falling)
        {
            playerGas -= Time.deltaTime * GasMptCalculator();
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
        playerVelocityLimiter.restMaxVelocity -= velocityDecreaseFactor / 1.3f;
        
        
        gameManager.playerMovementSpeed -= levelCalculatedParams.controlForce * gameSkillOverallModif;
        gameManager.fallingHelperPushForce -= levelCalculatedParams.fallingPushForce * gameSkillOverallModif;

        if (gameManager.playerMovementSpeed <= 0)
        {
            gameManager.playerMovementSpeed = 0;
        }

        if (gameManager.fallingHelperPushForce <= 0)
        {
            gameManager.fallingHelperPushForce = 0;
        }
        
        if (playerVelocityLimiter.pelvisMaxVelocity <= 0.05f)
        {
            playerVelocityLimiter.pelvisMaxVelocity = 0.05f;
            playerVelocityLimiter.velocitySmoother = playerVlocitySmoother;
            playerPuppetMaster.state = PuppetMaster.State.Dead;
        }
        
        if (playerVelocityLimiter.restMaxVelocity <= 0.05f)
        {
            playerVelocityLimiter.restMaxVelocity = 0.05f;
            playerVelocityLimiter.restVelocitySmoother = playerVlocitySmoother;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TotalDistanceTraveledTracker();
        PlayerOverallModifController();
        UIDynamicUpdate();
    }
}
