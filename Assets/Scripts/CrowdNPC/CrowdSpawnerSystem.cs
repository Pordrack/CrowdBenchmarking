using Latios.Transforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CrowdNPC
{
    public partial class CrowdSpawnerSystem : SystemBase
    {
        NativeArray<Entity> _entities;
        //True if a manual spawn was triggered. Force the spawning if the crowd spawner has a spawn on start set to false. Only trigger the spawn once.
        private bool _forceSpawn;
        private int _forceSpawnAmount;
        protected override void OnCreate()
        {
            RequireForUpdate<CrowdSpawner>();
            UnityEngine.Debug.Log("CrowdSpawnerSystem created");
        }

        public void Spawn(int forceSpawnAmount=10)
        {
            _forceSpawn = true;
            _forceSpawnAmount = forceSpawnAmount;
            _entities.Dispose();
            Enabled = true;
        }

        protected override void OnUpdate()
        {
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var crowdSpawnerEntity=SystemAPI.GetSingletonEntity<CrowdSpawner>();
            if (!crowdSpawner.SpawnOnStart && _forceSpawn==false) return;
            int spawnCount=crowdSpawner.SpawnCount;
            if(_forceSpawn)
            {
                spawnCount=_forceSpawnAmount;
            }

            _entities = new NativeArray<Entity>(spawnCount,Allocator.Persistent);
            EntityManager.Instantiate(crowdSpawner.PrefabEntity, _entities);

            for (int i = 0; i < spawnCount; i++)
            {
                var currentEntity = _entities[i];
                var spawnAreaDimension = crowdSpawner.SpawnAreaDimensions;
                var randomPosVariation= new float3(UnityEngine.Random.Range(-0.5f*spawnAreaDimension.x, 0.5f*spawnAreaDimension.x), 0, UnityEngine.Random.Range(-0.5f*spawnAreaDimension.y, 0.5f*spawnAreaDimension.y));
                var newPos = crowdSpawner.SpawnerPosition + randomPosVariation;
                EntityManager.GetAspect<TransformAspect>(currentEntity).worldPosition=newPos;
            }
            crowdSpawner.SpawnOnStart = false;
            _forceSpawn = false;
            Enabled = false;
        }

        protected override void OnDestroy()
        {
            UnityEngine.Debug.Log("CrowdSpawnerSystem destroyed");
            _entities.Dispose();
        }
    }
}
