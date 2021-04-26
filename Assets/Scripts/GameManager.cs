using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Common config")] [SerializeField]
    private Vector3 sceneDefaultPosition;
    public GameObject pressSpaceButton;
    public GameObject dogsThoughts;
    public GameArcadeManager arcadeManager;
    public enum GameState
    {
        Beginning,
        Start,
        Falling,
        ToTheDepth,
        End
    };

    public enum GameControlls
    {
        Depth,
        Stairs
    };

    public GameState gameState = GameState.Beginning;
    public GameControlls gameControls = GameControlls.Depth;

    [SerializeField] private Vector3 sceneFallingOffset;
    public bool isCameraNeedToFollowTargetByX = false;
    public UnityEvent InitEvent;

    [Header("Fadeout Config")] public DOTweenAnimation doTweenAnimation;
    public bool isRestartRequested = false;

    [Header("Moving scene config")] [SerializeField]
    private GameObject gameCamera;

    [SerializeField] private GameObject movingSceneObject;
    [SerializeField] private float heightDamping = 1f;
    [SerializeField] public bool isSceneFollowingPlayerHeightRightNow = false;
    public bool isWaitingForHeightOffset = true;

    [Header("Moving camera config")] [SerializeField]
    private ObjectLocalPositionManager objectLocalPositionManager;

    [SerializeField] public GameObject playerObject;

    [Header("Player Config")]
    public PlayerVelocityLimiter playerVelocityLimiter;

    public Animator currentPlayerAnimator;
    public float playerMovementSpeed = 10f;
    private static readonly int Rolling = Animator.StringToHash("Rolling");
    public float fallingHelperPushForce = 100f;

    [Header("Player Rotation Controller")] Vector3 m_EulerAngleVelocity = new Vector3(0, 0, 100);
    [SerializeField] private float torqueSpeed = 300;
    [SerializeField] private bool isFlipProcessing = false;

    [Header("Scene preparations")] [SerializeField]
    private Rigidbody[] objectsToDisableKinematics;
    public string sceneToLoadName;

    private void Start()
    {
        InitEverything();
    }

    public void GameStateManager()
    {
        if (gameState == GameState.Beginning && objectLocalPositionManager.movePositionTargedIndex == 1 &&
            !objectLocalPositionManager.isCurrentlyMoving)
        {
            gameState = GameState.Start;
            dogsThoughts.SetActive(false);
        }
    }

    private void InitEverything()
    {
        sceneDefaultPosition = movingSceneObject.transform.position;
        playerObject = GameObject.FindWithTag("Player");
        if (Camera.main != null) gameCamera = Camera.main.gameObject;
        SetupPlayerAndStuffForGame();
    }

    private void SetupPlayerAndStuffForGame()
    {
        InitEvent.Invoke();

        if (objectsToDisableKinematics.Length > 0)
        {
            for (int i = 0; i < objectsToDisableKinematics.Length; i++)
            {
                objectsToDisableKinematics[i].isKinematic = false;
                objectsToDisableKinematics[i].collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            }
        }

        playerVelocityLimiter.gameObject.GetComponent<PlayerRootManager>().playerPuppetMaster.pinWeight = 0;
    }

    private void CameraLookAtPlayer()
    {
        if (gameState == GameState.Beginning)
        {
            return;
        }

        if (isSceneFollowingPlayerHeightRightNow)
        {
            var targetRotation =
                Quaternion.LookRotation(playerObject.transform.position - gameCamera.transform.position);
            gameCamera.transform.rotation =
                Quaternion.Slerp(gameCamera.transform.rotation, targetRotation, 9 * Time.deltaTime);
        }
        else
        {
            gameCamera.transform.localEulerAngles = Vector3.zero;
        }
    }

    private void SceneReloadController()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isRestartRequested)
        {
            RestartLevelToggle();
        }
    }

    public void RestartLevelToggle()
    {
        if (gameState == GameState.Falling)
        {
            objectLocalPositionManager.ToggleUiHide();
        }

        isRestartRequested = true;
        doTweenAnimation.DOPlayBackwards();
    }

    public void RestartLevel()
    {
        if (isRestartRequested)
        {
            if (sceneToLoadName != "" && sceneToLoadName != null)
            {
                SceneManager.LoadScene(sceneToLoadName);
            }
            else
            {
                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);   
            }
        }
    }
    
    public void SwitchScenes(string sceneName)
    {
        Debug.Log("Scene switch asked");
        if (gameState == GameState.Falling)
        {
            objectLocalPositionManager.ToggleUiHide();
        }

        isRestartRequested = true;
        sceneToLoadName = sceneName;
        doTweenAnimation.DOPlayBackwards();
    }

    private void CharacterAirControll()
    {
        if (isSceneFollowingPlayerHeightRightNow && !objectLocalPositionManager.isCurrentlyMoving)
        {
            if (Input.GetKeyDown(KeyCode.Space) && !isFlipProcessing)
            {
                isFlipProcessing = true;
                currentPlayerAnimator.SetBool(Rolling, true);
                //StartCoroutine(CharacterFlipOne(1));
            }
            else if (Input.GetKeyUp(KeyCode.Space) && isFlipProcessing)
            {
                currentPlayerAnimator.SetBool(Rolling, false);
                isFlipProcessing = false;
            }

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            if (gameControls == GameControlls.Depth)
            {
                playerVelocityLimiter.pelvisVelocitySampler.AddForce(
                    transform.right * (playerMovementSpeed * Time.deltaTime * h));
                playerVelocityLimiter.pelvisVelocitySampler.AddForce(
                    transform.forward * (playerMovementSpeed * Time.deltaTime * v));
            }
            else if (gameControls == GameControlls.Stairs)
            {
                var hVectorNormalized = h < 0 ? h / 2 : h;
                var pushForceHelper = fallingHelperPushForce * arcadeManager.gameSkillOverallModif;
                var movementSpeedHelper = playerMovementSpeed * arcadeManager.gameSkillOverallModif;
                playerVelocityLimiter.pelvisVelocitySampler.AddForce(transform.right * (pushForceHelper * Time.deltaTime * hVectorNormalized));
                playerVelocityLimiter.pelvisVelocitySampler.AddForce(transform.up * (pushForceHelper / 4 * Time.deltaTime * -1));
                playerVelocityLimiter.pelvisVelocitySampler.AddForce(transform.forward * (movementSpeedHelper * Time.deltaTime * v));
            }
        }
    }

    IEnumerator CharacterFlipOne(float duration)
    {
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            yield return null;
        }

        currentPlayerAnimator.SetBool(Rolling, false);
        isFlipProcessing = false;
    }

    private void SceneFollowTargetXY()
    {
        var position = movingSceneObject.transform.position;
        var targetY = playerObject.transform.position.y + sceneFallingOffset.y;
        var lerpedY = Mathf.Lerp(position.y, targetY, heightDamping * Time.deltaTime);
        var lerpedX = position.x;
        if (isCameraNeedToFollowTargetByX)
        {
            var targetX = playerObject.transform.position.x + sceneFallingOffset.x;
            lerpedX = Mathf.Lerp(position.x, targetX, heightDamping * Time.deltaTime);
        }

        position = new Vector3(lerpedX, lerpedY, position.z);
        movingSceneObject.transform.position = position;
    }

    public void CameraSmoothFollowPlayerHeight()
    {
        if (isSceneFollowingPlayerHeightRightNow && !objectLocalPositionManager.isCurrentlyMoving)
        {
            if (movingSceneObject.transform.position.y > playerObject.transform.position.y + sceneFallingOffset.y)
            {
                SceneFollowTargetXY();
            }
            else if (!isWaitingForHeightOffset)
            {
                SceneFollowTargetXY();
            }
        }
    }

    public void MoveCameraToFallingMode()
    {
        gameState = GameState.ToTheDepth;
        currentPlayerAnimator.SetTrigger("Falling");
        isSceneFollowingPlayerHeightRightNow = true;
        objectLocalPositionManager.SetNewDestination(1);
    }

    public void MoveCameraToStairsMode()
    {
        gameState = GameState.Falling;
        objectLocalPositionManager.ToggleUiMovement();
        currentPlayerAnimator.SetTrigger("Falling");
        isSceneFollowingPlayerHeightRightNow = true;
        objectLocalPositionManager.SetNewDestination(2);
        var totalTimesPlayed = PlayerPrefs.GetFloat("TimesPlayed");
        PlayerPrefs.SetFloat("TimesPlayed", totalTimesPlayed + 1);
        arcadeManager.SetupLevelConfigurationBasedOnPlayerLevel();
    }

    public void MoveCameraToDefaultMode()
    {
        objectLocalPositionManager.SetNewDestination(1);
    }

    public void MoveCameraToStartMode()
    {
        objectLocalPositionManager.SetNewDestination(0);
    }

    private void FirstSpaceController()
    {
        if (gameState == GameState.Beginning && !objectLocalPositionManager.isCurrentlyMoving)
        {
            if (Input.GetKeyDown("space"))
            {
                MoveCameraToDefaultMode();
            }
        } else if (gameState == GameState.Start && !objectLocalPositionManager.isCurrentlyMoving)
        {
            if (Input.GetKeyDown("space"))
            {
                if (pressSpaceButton)
                {
                    pressSpaceButton.SetActive(false);
                }
            }
        }
    }

    private void Awake()
    {
        var backGroundMusicManager = playerObject = GameObject.FindWithTag("BackgroundMusicManager");
        if (backGroundMusicManager)
        {
            backGroundMusicManager.GetComponent<BackgroundMusicManager>().gameManager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GameStateManager();
        SceneReloadController();
        FirstSpaceController();
        CharacterAirControll();
    }

    private void FixedUpdate()
    {
        CameraLookAtPlayer();
        CameraSmoothFollowPlayerHeight();
    }
}