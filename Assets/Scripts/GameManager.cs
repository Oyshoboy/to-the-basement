using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Moving scene config")] [SerializeField]
    private GameObject gameCamera;

    [SerializeField] private GameObject movingSceneObject;
    [SerializeField] private Transform cameraDefaultPosition;
    [SerializeField] private Transform cameraFallingPosition;
    [SerializeField] private float heightDamping = 1f;
    [SerializeField] public bool isSceneFollowingPlayerHeight = false;

    [Header("Moving camera config")] [SerializeField]
    private float transitionSpeed = 0.5f;

    [SerializeField] public GameObject playerObject;

    [Header("Common config")] [SerializeField]
    private Vector3 sceneDefaultPosition;

    [SerializeField] private Vector3 sceneFallingOffset;
    [SerializeField] private bool isAnimationBeingMoved = false;
    public Animator currentPlayerAnimator;
    public bool isCameraFollowingTargetByX = false;

    [Header("Player Config")] [SerializeField]
    private PlayerVelocityLimiter playerVelocityLimiter;

    public float moveSpeed = 10f;
    private static readonly int Rolling = Animator.StringToHash("Rolling");
    
    [Header("Player Rotation Controller")]
    Vector3 m_EulerAngleVelocity = new Vector3(0, 0, 100);

    [SerializeField] private float torqueSpeed = 300;
    [SerializeField] private bool isFlipProcessing = false;
    private void Start()
    {
        sceneDefaultPosition = movingSceneObject.transform.position;
        playerObject = GameObject.FindWithTag("Player");
        if (Camera.main != null) gameCamera = Camera.main.gameObject;
    }

    private void CameraLookAtPlayer()
    {
        if (isSceneFollowingPlayerHeight)
        {
            gameCamera.transform.LookAt(playerObject.transform);
        }
        else
        {
            gameCamera.transform.localEulerAngles = Vector3.zero;
        }
    }

    private void SceneReloadController()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    private void CharacterAirControll()
    {

        if (isSceneFollowingPlayerHeight && !isAnimationBeingMoved)
        {
            
            if (Input.GetKeyDown(KeyCode.Space) && !isFlipProcessing)
            {
                isFlipProcessing = true;
                currentPlayerAnimator.SetBool(Rolling, true);
                //StartCoroutine(CharacterFlipOne(1));
            } else if (Input.GetKeyUp(KeyCode.Space) && isFlipProcessing)
            {
                currentPlayerAnimator.SetBool(Rolling, false);
                isFlipProcessing = false;
            }
            
            
            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");

            //var playerVelocityLimiterPos = playerVelocityLimiter.pelvisVelocitySampler.transform;
            //playerVelocityLimiter.pelvisVelocitySampler.MovePosition(
            //    playerVelocityLimiterPos.position + (playerVelocityLimiterPos.right * h + playerVelocityLimiterPos.up * v) * moveSpeed);
            playerVelocityLimiter.pelvisVelocitySampler.AddForce(transform.right * (moveSpeed * Time.deltaTime * h));
            playerVelocityLimiter.pelvisVelocitySampler.AddForce(transform.forward * (moveSpeed * Time.deltaTime * v));
        }
    }
    
    
    private void FlipController()
    {
        if (isFlipProcessing)
        {
            return;
            Quaternion deltaRotation = Quaternion.Euler(m_EulerAngleVelocity * (Time.fixedDeltaTime * torqueSpeed));
            playerVelocityLimiter.pelvisVelocitySampler.MoveRotation(playerVelocityLimiter.pelvisVelocitySampler.rotation * deltaRotation);
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

    IEnumerator MoveCameraToTarget(Vector3 origin, Vector3 target, float duration)
    {
        isAnimationBeingMoved = true;
        float journey = 0f;
        while (journey <= duration)
        {
            journey = journey + Time.deltaTime;
            float percent = Mathf.Clamp01(journey / duration);

            gameCamera.transform.localPosition = Vector3.Lerp(origin, target, percent);

            yield return null;
        }

        isAnimationBeingMoved = false;
    }

    public void CameraSmoothFollowPlayerHeight()
    {
        if (isSceneFollowingPlayerHeight && !isAnimationBeingMoved && movingSceneObject.transform.position.y >
            playerObject.transform.position.y + sceneFallingOffset.y)
        {
            var position = movingSceneObject.transform.position;
            var lerpedHeight = Mathf.Lerp(position.y, playerObject.transform.position.y + sceneFallingOffset.y,
                heightDamping * Time.deltaTime);

            var lerpedX = position.x;
            if (isCameraFollowingTargetByX)
            {
                lerpedX = Mathf.Lerp(position.x, playerObject.transform.position.x + sceneFallingOffset.x,
                    heightDamping * Time.deltaTime);
            }
            position = new Vector3(lerpedX, lerpedHeight, position.z);
            movingSceneObject.transform.position = position;
        }
    }

    public void MoveCameraToFallingMode()
    {
        isSceneFollowingPlayerHeight = true;
        StartCoroutine(MoveCameraToTarget(gameCamera.transform.localPosition, cameraFallingPosition.localPosition,
            transitionSpeed));
    }

    public void MoveCameraToDefaultMode()
    {
        StartCoroutine(MoveCameraToTarget(gameCamera.transform.localPosition, cameraDefaultPosition.localPosition,
            transitionSpeed));
    }


    // Update is called once per frame
    void Update()
    {
        CameraLookAtPlayer();
        SceneReloadController();
        CharacterAirControll();
    }

    private void FixedUpdate()
    {
        CameraSmoothFollowPlayerHeight();
        FlipController();
    }
}