using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CrowdNPC
{
    //The authoring composant can be used outside of ETS to spawn "regular" gameobjects
    public class CrowdSpawnerAuthoring : MonoBehaviour
    {
        public int SpawnCount;
        public GameObject Prefab;
        public float IndividualRadius;
        public Vector2 SpawnAreaDimensions;
        public bool SpawnOnStart = true;

        #region Singleton pattern
        public static CrowdSpawnerAuthoring Instance { get; private set; }
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        #endregion

        public void Start()
        {
            if (SpawnOnStart)
                Spawn();
        }

        public void Spawn()
        {
            if(!gameObject.activeInHierarchy) return;
            for (int i = 0; i < SpawnCount; i++)
            {
                Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-0.5f * SpawnAreaDimensions.x, 0.5f * SpawnAreaDimensions.x), 0, UnityEngine.Random.Range(-0.5f * SpawnAreaDimensions.y, 0.5f * SpawnAreaDimensions.y));
                Instantiate(Prefab, transform.position + spawnPosition, Quaternion.identity, transform);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnAreaDimensions.x, 0, SpawnAreaDimensions.y));
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, IndividualRadius);
        }
#endif

        public class Baker : Baker<CrowdSpawnerAuthoring>
        {
            public override void Bake(CrowdSpawnerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity,new CrowdSpawner
                {
                    PrefabEntity = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                    SpawnCount = authoring.SpawnCount,
                    IndividualRadius = authoring.IndividualRadius,
                    SpawnAreaDimensions = authoring.SpawnAreaDimensions,
                    SpawnerPosition = new float3(authoring.transform.position.x, authoring.transform.position.y, authoring.transform.position.z),
                    SpawnOnStart=authoring.SpawnOnStart,
                });
            }
        }
    }

    public partial struct CrowdSpawner : IComponentData
    {
        public Entity PrefabEntity;
        public int SpawnCount;
        public float IndividualRadius;
        public float2 SpawnAreaDimensions;
        public float3 SpawnerPosition;
        public bool SpawnOnStart;
    }
}

