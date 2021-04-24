using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistonController : MonoBehaviour
{
    [Header("Piston offsets")]
    [Range(0.1f, 1f)]
    public float maxExtension;
    [SerializeField] private GameObject pistonTargetPoint;

    [Header("Physics & params")]
    [SerializeField] private Rigidbody movingPart;
    [SerializeField] private float smoothTime = 0.3f;
    [SerializeField] private Vector3 velocity = Vector3.zero;
    public bool isPistonExtending = false;
    public bool isPistonActive = true;
    
    void Start()
    {
        if (movingPart == null)
        {
            Debug.LogError("Rigidbody cannot be empty!");
        }
    }
    
    
    public void PistonExtensionController()
    {
        var tempPos = Vector3.zero;
        if (isPistonExtending && isPistonActive)
        {
            tempPos.y = maxExtension;
            pistonTargetPoint.transform.localPosition = Vector3.SmoothDamp(pistonTargetPoint.transform.localPosition, tempPos, ref velocity, smoothTime);
        } else
        {
            pistonTargetPoint.transform.localPosition = Vector3.SmoothDamp(pistonTargetPoint.transform.localPosition, tempPos, ref velocity, smoothTime);
        }
    }

    public void PistonExtensionToggleController()
    {
        if (Input.GetKeyDown("space") && !isPistonExtending)
        {
            isPistonExtending = true;
        }
        if(Input.GetKeyUp("space") && isPistonExtending)
        {
            isPistonExtending = false;
        }
        
        PistonExtensionController();
    }
    
    void Update()
    {
        PistonExtensionToggleController();
        movingPart.MovePosition(pistonTargetPoint.transform.position);
    }
    
    
}