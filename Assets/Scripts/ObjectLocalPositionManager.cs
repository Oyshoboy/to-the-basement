using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DestinationsWithConfigs
{
    public Transform destination;
    public bool isRotatingWhileMoving = true;
    public float timeToMove = 2f;
}
public class ObjectLocalPositionManager : MonoBehaviour
{
    public DestinationsWithConfigs[] destinations;
    public GameObject objectToMove;
    [SerializeField] private float timeElapsed = 0;
    private Vector3 objectStartPosition;
    private Quaternion objectStartRotation;
    public int movePositionTargedIndex = 0;
    public bool isCurrentlyMoving = false;

    private void Start()
    {
        timeElapsed = float.MaxValue;
    }

    void MoveToPositionController()
    {
        if (timeElapsed < destinations[movePositionTargedIndex].timeToMove)
        {
            var selectedDestination = destinations[movePositionTargedIndex];
            var timeToMove = selectedDestination.timeToMove;
            var destination = selectedDestination.destination;
            var percent = timeElapsed / timeToMove;
            timeElapsed += Time.deltaTime / timeToMove;
            var smoothStep = Mathf.SmoothStep(0, 1, percent);
            objectToMove.transform.localPosition = Vector3.Lerp(objectStartPosition, destination.localPosition, smoothStep);
            if (selectedDestination.isRotatingWhileMoving)
            {
                objectToMove.transform.localRotation = Quaternion.Lerp(objectStartRotation, destination.localRotation, smoothStep);
            }
            isCurrentlyMoving = true;
        }
        else
        {
            isCurrentlyMoving = false;
        }
    }

    public void SetNewDestination(int destinationIndex)
    {
        objectStartPosition = objectToMove.transform.localPosition;
        objectStartRotation = objectToMove.transform.localRotation;
        movePositionTargedIndex = destinationIndex;
        timeElapsed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        MoveToPositionController();
    }
}
