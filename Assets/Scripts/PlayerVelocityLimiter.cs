using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVelocityLimiter : MonoBehaviour
{
    [Header("Pelvis Configuration")]
    public Rigidbody pelvisVelocitySampler;

    [Range(0.1f, 0.99f)]
    public float velocitySmoother = 0.95f;
    public float pelvisMaxVelocity = 400f;
    [SerializeField] private float currentVelocity = 0;

    [Header("Rest Rigidbodies")]
    [SerializeField]
    private Rigidbody[] rigidbodies;

    [Range(0.1f, 0.99f)]
    public float restVelocitySmoother = 0.95f;
    public float restMaxVelocity = 650f;
    public GameObject playerRagdoll;
    // Start is called before the first frame update
    void Start()
    {
        rigidbodies = playerRagdoll.GetComponentsInChildren<Rigidbody>();
    }

    private void rigidbodyVelocityLimitter(Rigidbody rb, float maxVelocity, float smooth)
    {
        currentVelocity = rb.velocity.sqrMagnitude;
        if (rb.velocity.sqrMagnitude > maxVelocity)
        {
            rb.velocity *= smooth;
        }      
    }

    private void RestRigidBodiesVelocityLimitter()
    {
        if (rigidbodies.Length < 1)
        {
            return;
        }

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodyVelocityLimitter(rigidbodies[i], restMaxVelocity, restVelocitySmoother);
        }
    }

    // Update is called once per frame
    void Update()
    {
        rigidbodyVelocityLimitter(pelvisVelocitySampler, pelvisMaxVelocity, velocitySmoother);
    }
}
