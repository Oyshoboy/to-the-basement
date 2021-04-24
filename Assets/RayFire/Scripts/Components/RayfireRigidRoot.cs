using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RayFire
{
    [Serializable]
    public class RFRigidRootDemolition
    {
        [Space (2)]
        public RFLimitations limitations = new RFLimitations();
        [Space (2)]
        public RFDemolitionMesh meshDemolition = new RFDemolitionMesh();
        [Space (2)]
        public RFDemolitionCluster clusterDemolition = new RFDemolitionCluster();
        [Space (2)]
        public RFSurface materials = new RFSurface();
    }

    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu ("RayFire/Rayfire Rigid Root")]
    [HelpURL ("http://rayfirestudios.com/unity-online-help/unity-rigid-root-component/")]
    public class RayfireRigidRoot : MonoBehaviour
    {
        public enum InitType
        {
            ByMethod = 0,
            AtStart  = 1
        }
        
        [Space (2)]
        public InitType initialization = InitType.AtStart;
        
        [Header ("  Simulation")]
        [Space (3)]
        
        public SimType simulationType = SimType.Dynamic;
        [Space (2)]
        
        public RFPhysic     physics    = new RFPhysic();
        [Space (2)]
        
        public RFActivation activation = new RFActivation();
        [Space (2)]

        
        
        [Header ("  Demolition")]
        [Space (3)]

        public RFRigidRootDemolition demolition = new RFRigidRootDemolition();
        
        //[Header ("  Shard Demolition Props TODO")]
        //[Space (3)]
        
        [Header ("  Common")]
        [Space (3)]
        
        public RFFade       fading     = new RFFade();
        [Space (2)]
        public RFReset      reset      = new RFReset();
        
        
        [HideInInspector] public RFCluster       cluster;
        
        
        
        [HideInInspector] public List<RFCluster> clusters;
        
        [HideInInspector] public List<RFShard> inactiveShards;
        
        [HideInInspector] public List<RFShard> rigidShards;
        
        [NonSerialized] List<RFShard> destroyShards;
        
        //[HideInInspector] 
        public bool  initialized;
       // [HideInInspector] 
        public bool  cached;
        [HideInInspector] public float sizeBox;
        [HideInInspector] public float sizeSum;
        
        // Components
        [HideInInspector] public Transform               tm;
        [HideInInspector] public RayfireSound            sound;
        [HideInInspector] public RayfireConnectivity     connect;
        [HideInInspector] public List<RayfireDebris>     debrisList;
        [HideInInspector] public List<RayfireDust>       dustList;
        [HideInInspector] public List<RayfireUnyielding> unyList;

        
        
        static          string strRoot = "RayFire Rigid Root: ";
        /// /////////////////////////////////////////////////////////
        /// Common
        /// /////////////////////////////////////////////////////////

        
        // Awake
        void Awake()
        {
            if (initialization == InitType.AtStart)
            {
                Initialize();
            }
            

            // TODO ACTIVATOR ActivationCheck fix for both types!!!!!!!!!!
            
            // TODO set sim state for fragments (field not serialized)
            
            // TODO init shards initPos at init even if setup 
            
            // TODO do not check for rigid UNy state, check for shard uny instead
        }
        
        /// /////////////////////////////////////////////////////////
        /// Enable/Disable
        /// /////////////////////////////////////////////////////////

        // Disable
        void OnDisable()
        {
            // Set coroutines states
            activation.inactiveCorState      = false;
        }

        // Activation
        void OnEnable()
        {
            if (gameObject.activeSelf == true && initialized == true)
                StartAllCoroutines();
        }
        
        // Initialize 
        public void Initialize()
        {
            if (initialized == false)
            {
                AwakeMethods();
                
                // Init sound
                RFSound.InitializationSound(sound, cluster.bound.size.magnitude);
                
            }
        }

        // Init connectivity if has
        void InitConnectivity()
        {
            if (connect != null)
            {
                // Set components
                connect.rigidHost     = null;
                connect.rigidRootHost = this;
                
                // Set by children.
                connect.SetClusterRigidRoot();
            
                // Start all coroutines
                connect.StartAllCoroutines();
            }
        }
        
        // Reset object
        public void ResetRigidRoot()
        {
            RFReset.ResetRigidRoot (this);
        }

        /// /////////////////////////////////////////////////////////
        /// Setup
        /// /////////////////////////////////////////////////////////

        public void ResetRoot()
        {
            // Reset
            cluster        = new RFCluster();
            inactiveShards = new List<RFShard>();
            destroyShards  = new List<RFShard>();
            
            // Reset connectivity shards
            if (connect != null)
                connect.ResetCluster();

            connect       = null;
            sound         = null;
            debrisList    = null;
            dustList      = null;
            unyList       = null;
            destroyShards = null;

            cached = false;
            
            // TODO Reset colliders
        }
        
        public void SetupRoot()
        {
            // Reset
            ResetRoot();
            
            // Set components
            SetComponents();
                
            // Set new cluster and set shards components
            SetShards();
            
            // Set shard colliders
            SetColliders();
            
            // Set unyielding shards
            SetUnyielding();
            
            // Set by children.
            connect = GetComponent<RayfireConnectivity>();
            if (connect != null)
            {
                connect.rigidHost     = null;
                connect.rigidRootHost = this;
                connect.SetClusterRigidRoot();
            }
            
            // Ignore collision
            if (physics.ignoreNeighbour == true)
            {
                physics.ignoreList = RFPhysic.GetIgnoreList(cluster.shards);
                Debug.Log (physics.ignoreList.Count);
            }

            cached = true;
        }
        
        // Awake ops
        void AwakeMethods()
        {
            // Create RayFire manager if not created
            RayfireMan.RayFireManInit();

            // Integrity check
            if (RFCluster.IntegrityCheck (cluster) == false)
                cached = false;
            
            // Set components
            SetComponents();
            
            // Do if not cached
            if (cached == false)
            {
                // Set shards components 
                SetShards();

                // Set shard colliders
                SetColliders();
            }

            // Set unyielding shards
            SetUnyielding();
            
            // Set colliders material
            SetCollidersMaterial();
            
            // Set components for mesh / skinned mesh / clusters
            SetPhysics();
            
            // Setup list for activation
            SetInactive ();

            // Set Particle Components: Initialize, collect
            RFParticles.SetParticleComponents(this);
                        
            // Set debris collider material
            RFPhysic.SetParticleColliderMaterial (debrisList);

            // Ignore collision
            RFPhysic.SetIgnoreColliders(physics, cluster.shards);
            
            // Start all necessary coroutines
            StartAllCoroutines();

            // Initialize connectivity
            InitConnectivity();

            //float t1 = Time.realtimeSinceStartup;
            
            
            //Debug.Log (Time.realtimeSinceStartup - t1);
            
            // Object initialized
            initialized            = true;

            // TODO Fade destroyShards
        }
        
        // Define basic components
        void SetComponents()
        {
            // Components
            tm      = GetComponent<Transform>();
            connect = GetComponent<RayfireConnectivity>();
            unyList = GetComponents<RayfireUnyielding>().ToList();
            
            // Set sound
            sound = GetComponent<RayfireSound>();
            if (sound != null)
            {
                sound.rigidRoot = this;
                sound.Check();
            }
        }
        
        // Set shards components
        void SetShards()
        { 
            // Get children
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < tm.childCount; i++)
                children.Add (tm.GetChild (i));

            // Get rigid root sim state
            SimType simState = simulationType;
            
            // Lists
            destroyShards = new List<RFShard>();
            
            // Set new cluster
            cluster = new RFCluster();
            for (int i = 0; i < children.Count; i++)
            {
                // Skip inactive children
                if (children[i].gameObject.activeSelf == false)
                    continue;
                
                // Check if already has rigid
                RayfireRigid rigid = children[i].gameObject.GetComponent<RayfireRigid>();
                
                // has own rigid
                if (rigid != null)
                {
                    // TODO
                    continue;
                    
                    // Init
                    rigid.Initialize();
                    
                    // Mesh
                    if (rigid.objectType == ObjectType.Mesh)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.rigid = rigid;
                        shard.mf    = shard.rigid.meshFilter;
                        cluster.shards.Add (shard);
                    }
                    
                    // Mesh Root
                    if (rigid.objectType == ObjectType.MeshRoot)
                    {
                        if (rigid.fragments.Count > 0)
                        {
                            for (int j = 0; j < rigid.fragments.Count; j++)
                            {
                                RFShard shard = new RFShard (rigid.fragments[j].transform, i); // TODO Set if considering all shard ids
                                shard.rigid = rigid.fragments[j];
                                cluster.shards.Add (shard);
                            }
                        }
                    }
                    
                    // Connected Cluster TODO 
                    if (rigid.objectType == ObjectType.ConnectedCluster || rigid.objectType == ObjectType.NestedCluster)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.rigid = rigid;
                        cluster.shards.Add (shard);
                    }
                }

                // Has no own rigid
                if (rigid == null)
                {
                    // Mesh
                    if (children[i].childCount == 0)
                    {
                        RFShard shard = new RFShard (children[i].transform, i);
                        shard.mf = children[i].transform.GetComponent<MeshFilter>();
                       
                        // Filter
                        if (ShardFilter(shard, this) == false)
                            continue;

                        // Get components
                        shard.sm  = simState;
                        shard.rb  = children[i].transform.GetComponent<Rigidbody>();
                        shard.col = children[i].transform.GetComponent<Collider>(); 
                        // TODO check if Mesh Collider with convex off and kinematic sim type: conflict
                        
                        // Collect
                        cluster.shards.Add (shard);
                    }

                    // Mesh Root TODO
                    else if (children[i].childCount > 0)
                    {
                        if (IsNestedCluster (children[i]) == true)
                        {
                             // Nested
                        }
                        else
                        {
                            // Connected
                        }
                    }
                }
            }
            
            // Set shards id TODO exclude all shards without meshfilter
            for (int j = 0; j < cluster.shards.Count; j++)
            {
                cluster.shards[j].id = j;
            }
            
            // Set bound if has not
            cluster.bound = RFCluster.GetShardsBound (cluster.shards);
        }
        
        // Define collider
        void SetColliders()
        {
            for (int i = 0; i < cluster.shards.Count; i++)
                if (cluster.shards[i].rigid == null)
                    RFPhysic.SetCollider (this, cluster.shards[i]);
        }
        
        // Define components
        void SetCollidersMaterial()
        {
            // Set material solidity and destructible
            physics.solidity     = physics.Solidity;
            physics.destructible = physics.Destructible;
            
            // Set physics material if not defined by user
            if (physics.material == null)
                physics.material = physics.PhysMaterial;
            
            // Add Collider and Rigid body if has no Rigid component
            for (int i = 0; i < cluster.shards.Count; i++)
                if (cluster.shards[i].rigid == null)
                    RFPhysic.SetColliderMaterial (this, cluster.shards[i]);
        }
        
        // Define components
        void SetPhysics()
        {
            // Set density.
            float density     = RayfireMan.inst.materialPresets.Density (physics.materialType);
            float drag        = RayfireMan.inst.materialPresets.Drag (physics.materialType);
            float dragAngular = RayfireMan.inst.materialPresets.AngularDrag (physics.materialType);
            
            // Add Collider and Rigid body if has no Rigid component
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                // Has no own rigid component: add collider and rigidbody
                if (cluster.shards[i].rigid == null)
                {
                    // Set Rigid body
                    if (cluster.shards[i].rb == null)
                        cluster.shards[i].rb = cluster.shards[i].tm.gameObject.AddComponent<Rigidbody>();

                    // Set simulation
                    RFPhysic.SetSimulationType (cluster.shards[i].rb, simulationType, ObjectType.Mesh, physics.useGravity, physics.solverIterations);
                    
                    // Set density. After collider defined
                    RFPhysic.SetDensity (this, cluster.shards[i], density);

                    // Set drag properties
                    RFPhysic.SetDrag (cluster.shards[i], drag, dragAngular);
                }
            }
        }
        
        // Setup inactive shards
        void SetInactive()
        {
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
            {
                inactiveShards = new List<RFShard>();
                for (int i = 0; i < cluster.shards.Count; i++)
                {
                    cluster.shards[i].pos = cluster.shards[i].tm.position;
                    inactiveShards.Add (cluster.shards[i]);
                }
            }
        }
        
        // Set unyielding shards
        void SetUnyielding()
        {
            // Set by rigid root
            for (int i = 0; i < cluster.shards.Count; i++)
            {
                cluster.shards[i].uny = activation.unyielding;
                cluster.shards[i].act = activation.activatable;
            }

            // Set by uny components
            if (unyList != null && unyList.Count > 0)
                for (int i = 0; i < unyList.Count; i++)
                    unyList[i].SetRigidRootUnyByOverlap (this);
        }
        
        // Start all coroutines
        void StartAllCoroutines()
        {
            // Stop if static
            if (simulationType == SimType.Static)
                return;
            
            // Inactive
            if (gameObject.activeSelf == false)
                return;
            
            // Prevent physics cors
            if (physics.exclude == true)
                return;
            
            // Init inactive every frame update coroutine
            if (simulationType == SimType.Inactive || simulationType == SimType.Kinematic)
                StartCoroutine (activation.InactiveCor(this));
        }
        
        // Prepare shards. Set bounds, set neibs
        static void SetShardsByRigids(RFCluster cluster, List<RayfireRigid> rigidList, ConnectivityType connectivity)
        {
            for (int i = 0; i < rigidList.Count; i++)
            {
                // Get mesh filter
                MeshFilter mf = rigidList[i].GetComponent<MeshFilter>();

                // Child has no mesh
                if (mf == null)
                    continue;

                // Create new shard
                RFShard shard = new RFShard(rigidList[i].transform, i);
                shard.cluster = cluster;
                shard.rigid   = rigidList[i];
                shard.uny     = rigidList[i].activation.unyielding;
                shard.act     = rigidList[i].activation.activatable;
                shard.col     = rigidList[i].physics.meshCollider;

                // Set faces data for connectivity
                if (connectivity == ConnectivityType.ByMesh)
                    RFTriangle.SetTriangles(shard, mf);

                // Collect shard
                cluster.shards.Add(shard);
            }
        }
        
        /// /////////////////////////////////////////////////////////
        /// Inactive
        /// /////////////////////////////////////////////////////////
        
        // Activate shard by collider
        public void ActivateCollider (Collider coll)
        {
            for (int i = inactiveShards.Count - 1; i >= 0; i--)
            {
                if (inactiveShards[i].col == coll)
                {
                    // Activate and remove if activated
                    if (RFActivation.ActivateShard (inactiveShards[i], this) == true)
                        inactiveShards.RemoveAt (i);
                    
                    // Break because collider matched shard
                    break;
                }
            }
        }
       
        /// /////////////////////////////////////////////////////////
        /// Children change
        /// /////////////////////////////////////////////////////////
        
        /*
         [NonSerialized] bool   childrenChanged;
         
        // Children change
        void OnTransformChildrenChanged()
        {
            childrenChanged = true; 
        }
        
        // Connectivity check cor
        IEnumerator ChildrenCor()
        {
            // Stop if running 
            if (childrenCorState == true)
                yield break;
            
            // Set running state
            childrenCorState = true;
            
            bool checkChildren = true;
            while (checkChildren == true)
            {
                // Get not connected groups
                if (childrenChanged == true)
                    connectivityCheckNeed = true;

                yield return null;
            }
            
            // Set state
            childrenCorState = false;
        }
        */
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Copy rigid root properties to rigid
        public void CopyPropertiesTo (RayfireRigid toScr)
        {
            // Object type
            toScr.objectType     = ObjectType.ConnectedCluster;
            toScr.demolitionType = DemolitionType.None;
            toScr.simulationType = SimType.Dynamic;
            
            // Copy physics
            toScr.physics.CopyFrom (physics);
            toScr.activation.CopyFrom (activation);
            
            
            toScr.limitations.CopyFrom (demolition.limitations);
            toScr.meshDemolition.CopyFrom (demolition.meshDemolition);
            toScr.clusterDemolition.CopyFrom (demolition.clusterDemolition);
            toScr.materials.CopyFrom (demolition.materials);
            
            
            //toScr.damage.CopyFrom (damage);
            toScr.fading.CopyFrom (fading);
            //toScr.reset.CopyFrom (this);
        }
        
        /// /////////////////////////////////////////////////////////
        /// Static
        /// /////////////////////////////////////////////////////////
        
        // Check if root is nested cluster
        static bool IsNestedCluster (Transform trans)
        {
            for (int c = 0; c < trans.childCount; c++)
                if (trans.GetChild (c).childCount > 0)
                    return true;
            return false;
        }
        
        // Shard filter
        static bool ShardFilter(RFShard shard, RayfireRigidRoot scr)
        {
            // No mesh filter
            if (shard.mf == null)
            {
                Debug.Log (strRoot + shard.tm.name + " has no MeshFilter. Shard won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }

            // No mesh
            if (shard.mf.sharedMesh == null)
            {
                Debug.Log (strRoot + shard.tm.name + " has no mesh. Shard won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }
            
            // Low vert check
            if (shard.mf.sharedMesh.vertexCount <= 3)
            {
                Debug.Log (strRoot + shard.tm.name + " has 3 or less vertices. Shard can't get Mesh Collider and won't be simulated.", shard.tm.gameObject);
                scr.destroyShards.Add (shard);
                return false;
            }
            
            // Size check
            if (RayfireMan.colliderSizeStatic > 0)
            {
                if (shard.sz < RayfireMan.colliderSizeStatic)
                {
                    Debug.Log (strRoot + shard.tm.name + " is very small and won't be simulated.", shard.tm.gameObject);
                    scr.destroyShards.Add (shard);
                    return false;
                }
            }

            // Optional coplanar check
            if (scr.physics.planarCheck == true && shard.mf.sharedMesh.vertexCount < RFPhysic.coplanarVertLimit)
            {
                if (RFShatterAdvanced.IsCoplanar (shard.mf.sharedMesh, RFShatterAdvanced.planarThreshold) == true)
                {
                    Debug.Log (strRoot + shard.tm.name + " has planar low poly mesh. Shard can't get Mesh Collider and won't be simulated.", shard.tm.gameObject);
                    scr.destroyShards.Add (shard);
                    return false;
                }
            }

            return true;
        }
        
        /// /////////////////////////////////////////////////////////
        /// Getters
        /// /////////////////////////////////////////////////////////
        
        public bool HasDebris { get { return debrisList != null && debrisList.Count > 0; } }
        public bool HasDust { get { return dustList != null && dustList.Count > 0; } }
        
        public void CollideTest()
        {
            /*
            List<Transform> tmList = new List<Transform>();
            for (int i = 0; i < transform.childCount; i++)
                tmList.Add (transform.GetChild (i));

            List<Collider> colliders = new List<Collider>();
            foreach (var tm in tmList)
            {
                Collider col = tm.GetComponent<Collider>();
                if (col == null)
                {
                    col                          = tm.gameObject.AddComponent<MeshCollider>();
                    (col as MeshCollider).convex = true;
                }
                colliders.Add (col);
            }

            */

           // Physics.Simulate (0.01f);
            Physics.autoSimulation = true;

            // Physics.autoSyncTransforms = false;



            // https://forum.unity.com/threads/physics-simulate-for-a-single-object-possible.614404/
            // https://forum.unity.com/threads/separating-physics-scenes.597697/
            // https://stackoverflow.com/questions/50693509/can-we-detect-when-a-rigid-body-collides-using-physics-simulate-in-unity

        }

        void OnCollisionEnter (Collision other)
        {
            Debug.Log (other.collider.name);
        }
    }
}
