using RootMotion.Dynamics;
using UnityEngine;

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
    public GameObject gameOverMenu;
    
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
    public bool isPlayerDead = false;
    public bool isGameOver = false;
    public bool resetPlayerLevelSkill = false;
    public SoundController soundController;
    
    //SYSTEM VARIABLES
    [SerializeField] private Vector3 playerStartPosition;
    [SerializeField] private float maxDistanceTraveled;
    
    // Start is called before the first frame update
    void Start()
    {
        playerStartPosition = playerPhysicsRoot.transform.position;
        playerVlocitySmoother = playerVelocityLimiter.velocitySmoother;
        SetupLevelConfigurationBasedOnPlayerLevel();
    }

    public void SetupLevelConfigurationBasedOnPlayerLevel()
    {
        var currentPlayerLevel = PlayerPrefs.GetFloat("PlayerSkillLevel");
        // = currentPlayerLevel > 8 ? 10 : currentPlayerLevel;
        var playerMaxLevl = playerMaxSkills.level;
        var playerExperiencCoef = currentPlayerLevel / playerMaxLevl;
        
        // VELOCITY CALCULATING
        var minMaxVelocityDifference = playerMaxSkills.maxVelocity - playerDefaultSkills.maxVelocity;
        var calculatedMaxVelocity = minMaxVelocityDifference * playerExperiencCoef;
        levelCalculatedParams.maxVelocity = playerDefaultSkills.maxVelocity + calculatedMaxVelocity;
        
        // MPG Calculating
        var minMaxMPGDifferece = playerMaxSkills.mpg - playerDefaultSkills.mpg;
        var calculatedMPG = minMaxMPGDifferece * playerExperiencCoef;
        levelCalculatedParams.mpg = playerDefaultSkills.mpg + calculatedMPG;
        
        //MPT Calculating
        var minMaxMPTDifferece = playerMaxSkills.mpt - playerDefaultSkills.mpt;
        var calculatedMPT = minMaxMPTDifferece * playerExperiencCoef;
        levelCalculatedParams.mpt = playerDefaultSkills.mpt + calculatedMPT;
        
        //Falling push force Calculating
        var minMaxPushForceDifferece = playerMaxSkills.fallingPushForce - playerDefaultSkills.fallingPushForce;
        var calculatedPushForce = minMaxPushForceDifferece * playerExperiencCoef;
        levelCalculatedParams.fallingPushForce = playerDefaultSkills.fallingPushForce + calculatedPushForce;
        
        //Control force Calculating
        var minMaxControlForceDifferece = playerMaxSkills.controlForce - playerDefaultSkills.controlForce;
        var calculatedControlForce = minMaxControlForceDifferece * playerExperiencCoef;
        levelCalculatedParams.controlForce = playerDefaultSkills.controlForce + calculatedControlForce;

        levelCalculatedParams.level = currentPlayerLevel;
        UpdateGamePhysicsSettingsBasedOnLevel();
    }

    private void UpdateGamePhysicsSettingsBasedOnLevel()
    {
        gameManager.playerMovementSpeed = levelCalculatedParams.controlForce;
        gameManager.fallingHelperPushForce = levelCalculatedParams.fallingPushForce;
        playerVelocityLimiter.pelvisMaxVelocity = levelCalculatedParams.maxVelocity;
        playerVelocityLimiter.restMaxVelocity = levelCalculatedParams.maxVelocity;
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
            if (maxDistanceTraveled > PlayerPrefs.GetFloat("MaxDistanceTraveled"))
            {
                PlayerPrefs.SetFloat("MaxDistanceTraveled", maxDistanceTraveled);
            }
        }
    }

    public void AddGase()
    {
        playerGas += levelCalculatedParams.npcGasPrice;
    }

    public string FloatToThreeDigitText(float value)
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
        if(uiTexts.Length < 1) return;
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

            if (gameSkillOverallModif >= 0.33f)
            {
                playerPuppetMaster.muscleWeight = gameSkillOverallModif;
            } else if (gameSkillOverallModif < 0.33f)
            {
                gameManager.currentPlayerAnimator.enabled = false;
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

        if (playerVelocityLimiter.pelvisMaxVelocity <= 5f)
        {
            playerVelocityLimiter.pelvisMaxVelocity = 5f;
        }
        
        if (playerVelocityLimiter.restMaxVelocity <= 5f)
        {
            playerVelocityLimiter.restMaxVelocity = 5f;
        }
    }

    private void GameOverController()
    {
        if (playerGas < 0.05f && playerVelocityLimiter.currentVelocity < 1f && !isGameOver)
        {
            isGameOver = true;
            if(!gameOverMenu) return;       
            Debug.Log("GAME OVER");
            soundController.PlayGameOverSound();
            gameOverMenu.SetActive(true);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        TotalDistanceTraveledTracker();
        PlayerOverallModifController();
        UIDynamicUpdate();
        GameOverController();
        
        if (resetPlayerLevelSkill)
        {
            PlayerPrefs.SetFloat("PlayerSkillLevel", 0);
        }

        if (isPlayerDead)
        {
            playerGas = 0;
        }
    }
}
