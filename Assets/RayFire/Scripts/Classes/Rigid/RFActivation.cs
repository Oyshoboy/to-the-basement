using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RayFire
{
    [Serializable]
    public class RFActivation
    {
        [Header ("  Activation")]
        [Space (3)]
        [Tooltip ("Inactive object will be activated when it's velocity will be higher than By Velocity value when pushed by other dynamic objects.")]
        public float byVelocity;

        [Space (1)]
        [Tooltip ("Inactive object will be activated if will be pushed from it's original position farther than By Offset value.")]
        public float byOffset;

        [Space (1)]
        [Tooltip ("Inactive object will be activated if will get total damage higher than this value.")]
        public float byDamage;

        [Space (1)]
        [Tooltip ("Inactive object will be activated by overlapping with object with RayFire Activator component.")]
        public bool byActivator;

        [Space (1)]
        [Tooltip ("Inactive object will be activated when it will be shot by RayFireGun component.")]
        public bool byImpact;

        [Space (1)]
        [Tooltip ("Inactive object will be activated by Connectivity component if it will not be connected with Unyielding zone.")]
        public bool byConnectivity;

        [Header ("  Connectivity")]
        [Space (3)]
        [Tooltip ("Allows to define Inactive/Kinematic object as Unyielding to check for connection with other Inactive/Kinematic objects with enabled By Connectivity activation type.")]
        public bool unyielding;
        [Space (1)]
        [Tooltip ("Unyielding object can not be activate by default. When On allows to activate Unyielding objects as well.")]
        public bool activatable;

        // Nom serialized
        [NonSerialized] public RayfireConnectivity connect;
        [NonSerialized] public List<int>           unyList;
        [NonSerialized] public bool                activated;
        [NonSerialized] public bool                inactiveCorState;

        /// /////////////////////////////////////////////////////////
        /// Constructor
        /// /////////////////////////////////////////////////////////

        // Constructor
        public RFActivation()
        {
            byVelocity     = 0f;
            byOffset       = 0f;
            byDamage       = 0f;
            byActivator    = false;
            byImpact       = false;
            byConnectivity = false;
            unyielding     = false;
            activatable    = false;
            activated      = false;

            // unyList        = new List<int>();
            Reset();
        }

        // Copy from
        public void CopyFrom (RFActivation act)
        {
            byActivator    = act.byActivator;
            byImpact       = act.byImpact;
            byVelocity     = act.byVelocity;
            byOffset       = act.byOffset;
            byDamage       = act.byDamage;
            byConnectivity = act.byConnectivity;
            unyielding     = act.unyielding;
            activatable    = act.activatable;
        }

        /// /////////////////////////////////////////////////////////
        /// Methods
        /// /////////////////////////////////////////////////////////

        // Turn of all activation properties
        public void Reset()
        {
            activated = false;
        }

        // Connectivity check
        public void CheckConnectivity()
        {
            if (byConnectivity == true && connect != null)
            {
                connect.connectivityCheckNeed = true;
                connect = null;
            }
        }

        /// /////////////////////////////////////////////////////////
        /// Coroutines
        /// /////////////////////////////////////////////////////////

        // Check velocity for activation
        public IEnumerator ActivationVelocityCor (RayfireRigid scr)
        {
            while (scr.activation.activated == false && scr.activation.byVelocity > 0)
            {
                if (scr.physics.rigidBody.velocity.magnitude > byVelocity)
                    scr.Activate();
                yield return null;
            }
        }

        // Check offset for activation
        public IEnumerator ActivationOffsetCor (RayfireRigid scr)
        {
            while (scr.activation.activated == false && scr.activation.byOffset > 0)
            {
                if (Vector3.Distance (scr.transForm.position, scr.physics.initPosition) > scr.activation.byOffset)
                    scr.Activate();
                yield return null;
            }
        }

        // Exclude from simulation, move under ground, destroy
        public IEnumerator InactiveCor (RayfireRigid scr)
        {
            // Stop if running 
            if (inactiveCorState == true)
                yield break;

            // Set running state
            inactiveCorState = true;

            //scr.transForm.hasChanged = false;
            while (scr.simulationType == SimType.Inactive)
            {
                //if (scr.transForm.hasChanged == true)
                {
                    scr.physics.rigidBody.velocity        = Vector3.zero;
                    scr.physics.rigidBody.angularVelocity = Vector3.zero;
                }
                yield return null;
            }

            // Set state
            inactiveCorState = false;
        }

         // Activation by velocity and offset
         public IEnumerator InactiveCor  (RayfireRigidRoot scr)
        {
            // Stop if running 
            if (inactiveCorState == true)
                yield break;

            // Set running state
            inactiveCorState = true;
            
            while (scr.inactiveShards.Count > 0)
            {
                // Timestamp
                // float t1 = Time.realtimeSinceStartup;
                
                // Remove activated shards
                for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                    if (scr.inactiveShards[i].tm == null || scr.inactiveShards[i].sm == SimType.Dynamic)
                        scr.inactiveShards.RemoveAt (i);

                // Velocity activation
                if (scr.activation.byVelocity > 0)
                {
                    for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                    {
                        if (scr.inactiveShards[i].tm.hasChanged == true)
                            if (scr.inactiveShards[i].rb.velocity.magnitude > scr.activation.byVelocity)
                                if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                    scr.inactiveShards.RemoveAt (i);
                    }

                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                        yield break;
                }

                // Offset activation
                if (scr.activation.byOffset > 0)
                {
                    for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                    {
                        if (scr.inactiveShards[i].tm.hasChanged == true)
                            if (Vector3.Distance (scr.inactiveShards[i].tm.position, scr.inactiveShards[i].pos) > scr.activation.byOffset)
                                if (ActivateShard (scr.inactiveShards[i], scr) == true)
                                    scr.inactiveShards.RemoveAt (i);
                    }

                    // Stop 
                    if (scr.inactiveShards.Count == 0)
                        yield break;
                }
                
                // Stop velocity
                for (int i = scr.inactiveShards.Count - 1; i >= 0; i--)
                {
                    if (scr.inactiveShards[i].tm.hasChanged == true)
                    {
                        scr.inactiveShards[i].rb.velocity        = Vector3.zero;
                        scr.inactiveShards[i].rb.angularVelocity = Vector3.zero;
                        scr.inactiveShards[i].tm.hasChanged      = false;
                    }
                }
                
                // Debug.Log (Time.realtimeSinceStartup - t1);
                
                // TODO repeat 30 times per second, not every frame
                yield return null;
            }
            
            // Set state
            inactiveCorState = false;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////

        // Activate inactive object
        public static void ActivateRigid (RayfireRigid scr, bool connCheck = true)
        {
            // Stop if excluded
            if (scr.physics.exclude == true)
                return;

            // Skip not activatable unyielding objects
            if (scr.activation.activatable == false && scr.activation.unyielding == true)
                return;

            // Initialize if not
            if (scr.initialized == false)
                scr.Initialize();

            // Turn convex if kinematic activation
            if (scr.simulationType == SimType.Kinematic)
            {
                MeshCollider meshCollider = scr.physics.meshCollider as MeshCollider;
                if (meshCollider != null)
                    meshCollider.convex = true;

                // Swap with animated object
                if (scr.physics.rec == true)
                {
                    // Set dynamic before copy
                    scr.simulationType                = SimType.Dynamic;
                    scr.physics.rigidBody.isKinematic = false;
                    scr.physics.rigidBody.useGravity  = scr.physics.useGravity;

                    // Create copy
                    GameObject inst = UnityEngine.Object.Instantiate (scr.gameObject);
                    inst.transform.position = scr.transForm.position;
                    inst.transform.rotation = scr.transForm.rotation;

                    // Save velocity
                    Rigidbody rBody = inst.GetComponent<Rigidbody>();
                    if (rBody != null)
                    {
                        rBody.velocity        = scr.physics.rigidBody.velocity;
                        rBody.angularVelocity = scr.physics.rigidBody.angularVelocity;
                    }

                    // Activate and init rigid
                    scr.gameObject.SetActive (false);
                }
            }

            // Connectivity check
            if (connCheck == true)
                scr.activation.CheckConnectivity();
            
            // Set state
            scr.activation.activated = true;

            // Set props
            scr.simulationType                = SimType.Dynamic;
            scr.physics.rigidBody.isKinematic = false; // TODO error at manual activation of stressed connectivity structure
            scr.physics.rigidBody.useGravity  = scr.physics.useGravity;

            // Fade on activation
            if (scr.fading.onActivation == true) 
                scr.Fade();

            // Parent
            if (RayfireMan.inst.parent != null)
                scr.gameObject.transform.parent = RayfireMan.inst.parent.transform;

            // Init particles on activation
            RFParticles.InitActivationParticlesRigid (scr);

            // Activation sound
            RFSound.ActivationSound (scr.sound, scr.limitations.bboxSize);

            // Events
            scr.activationEvent.InvokeLocalEvent (scr);
            RFActivationEvent.InvokeGlobalEvent (scr);

            // Add initial rotation if still TODO put in ui
            if (scr.physics.rigidBody.angularVelocity == Vector3.zero)
            {
                float val = 1.0f;
                scr.physics.rigidBody.angularVelocity = new Vector3 (
                    Random.Range (-val, val), Random.Range (-val, val), Random.Range (-val, val));
            }
        }

        // Activate Rigid Root shard
        public static bool ActivateShard (RFShard shard, RayfireRigidRoot rigidRoot)
        {
            // Skip not activatable unyielding shards
            if (shard.act == false && shard.uny == true)
                return false;
            
            // Already dynamic
            if (shard.sm == SimType.Dynamic)
                return false;
            
            // Set dynamic sim state
            shard.sm = SimType.Dynamic;
            
            // Activate by Rigid if has rigid
            if (shard.rigid != null)
            {
                ActivateRigid (shard.rigid);
                return true;
            }

            // Set props
            if (shard.rb.isKinematic == true)
                shard.rb.isKinematic = false;

            // Turn On Gravity
            shard.rb.useGravity = true;

            // Activation Fade
            if (rigidRoot.fading.onActivation == true)
                RFFade.FadeShard (rigidRoot, shard);

            // Parent
            if (RayfireMan.inst.parent != null)
                shard.tm.parent = RayfireMan.inst.parent.transform;

            // Connectivity check if shards was activated: TODO check only neibs of activated?
            if (rigidRoot.activation.byConnectivity == true && rigidRoot.activation.connect != null)
                rigidRoot.connect.connectivityCheckNeed = true;
            
            // Init particles on activation
            RFParticles.InitActivationParticlesShard(rigidRoot, shard);
            
            // Activation sound
            RFSound.ActivationSound (rigidRoot.sound, rigidRoot.cluster.bound.size.magnitude);
            
            // Add initial rotation if still TODO put in ui
            float val = 1.0f;
            if (shard.rb.angularVelocity == Vector3.zero)
                shard.rb.angularVelocity = new Vector3 (
                    Random.Range (-val, val), Random.Range (-val, val), Random.Range (-val, val));
            
            return true;
        }

        /// /////////////////////////////////////////////////////////
        /// Sliced activation
        /// /////////////////////////////////////////////////////////

        // CHeck for overlap with mesh Rigid
        public static void OverlapActivation (RayfireRigid scr)
        {
            // Only inactive and kinematic
            if (scr.simulationType != SimType.Inactive && scr.simulationType != SimType.Kinematic)
                return;

            // No fragments
            if (scr.fragments == null || scr.fragments.Count == 0)
                return;

            // No unyielding zones at all
            if (RayfireMan.inst.unyList.Count == 0)
                return;

            // Has no unyielding zone
            if (scr.activation.HasUny == false)
                return;

            // TODO collect layer mask by all layers -> int finalMask = RayfireUnyielding.ClusterLayerMask(scr);
            int layerMask = 1 << scr.fragments[0].gameObject.layer;
            
            // Overlapped objects: Copy uny, stay kinematic
            List<RayfireRigid> inObjects = new List<RayfireRigid>();

            // Get all overlapped fragments
            foreach (RFUny uny in RayfireMan.inst.unyList)
            {
                // Original object in local uny zone
                if (scr.activation.unyList.Contains (uny.id) == true)
                {
                    // Get box overlap colliders
                    Collider[]        colliders = Physics.OverlapBox (uny.center, uny.size, uny.rotation, layerMask);
                    HashSet<Collider> set       = new HashSet<Collider>(colliders);
                    
                    // Activate if do not overlap
                    for (int i = 0; i < scr.fragments.Count; i++)
                    {
                        // Activate not overlapped and copy to overlapped
                        if (set.Contains (scr.fragments[i].physics.meshCollider) == true)
                        {
                            // Copy overlap uny to overlapped object
                            RayfireUnyielding.SetRigidUnyState (scr.fragments[i], uny.id, scr.activation.unyielding, scr.activation.activatable);

                            // Already collected
                            if (inObjects.Contains (scr.fragments[i]) == true)
                                continue;

                            // Collect overlapped object
                            inObjects.Add (scr.fragments[i]);

                            // Set overlapped back to kinematic
                            if (scr.simulationType == SimType.Kinematic)
                            {
                                scr.fragments[i].simulationType = SimType.Kinematic;
                                RFPhysic.SetSimulationType (scr.fragments[i].physics.rigidBody, scr.fragments[i].simulationType, scr.objectType, scr.fragments[i].physics.useGravity, scr.fragments[i].physics.solverIterations);
                            }
                        }
                    }
                }
            }

            // Activate all not overlapped fragments
            foreach (var frag in scr.fragments)
                if (inObjects.Contains (frag) == false)
                {
                    frag.activation.unyielding = false;
                    frag.Activate();
                }
        }

        // Copy unyielding component
        static void CopyUny (RayfireUnyielding source, GameObject target)
        {
            RayfireUnyielding newUny = target.AddComponent<RayfireUnyielding>();

            // Copy position
            Vector3 globalCenter = source.transform.TransformPoint (source.centerPosition);
            newUny.centerPosition = newUny.transform.InverseTransformPoint (globalCenter);

            // Copy size
            newUny.size   =  source.size;
            newUny.size.x *= source.transform.localScale.x;
            newUny.size.y *= source.transform.localScale.y;
            newUny.size.z *= source.transform.localScale.z;
        }

        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////

        // Has uny zones
        public bool HasUny { get { return unyList != null && unyList.Count > 0; } }
    }
}