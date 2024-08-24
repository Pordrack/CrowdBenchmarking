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
    public partial struct CrowdSpawnerSystem : ISystem
    {
        NativeArray<Entity> _entities;
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CrowdSpawner>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var crowdSpawnerEntity=SystemAPI.GetSingletonEntity<CrowdSpawner>();
            _entities = new NativeArray<Entity>(crowdSpawner.SpawnCount,Allocator.Persistent);
            state.EntityManager.Instantiate(crowdSpawner.PrefabEntity, _entities);

            for (int i = 0; i < crowdSpawner.SpawnCount; i++)
            {
                var currentEntity = _entities[i];
                var spawnAreaDimension = crowdSpawner.SpawnAreaDimensions;
                var randomPosVariation= new float3(UnityEngine.Random.Range(-0.5f*spawnAreaDimension.x, 0.5f*spawnAreaDimension.x), 0, UnityEngine.Random.Range(-0.5f*spawnAreaDimension.y, 0.5f*spawnAreaDimension.y));
                var newPos = crowdSpawner.SpawnerPosition + randomPosVariation;
                state.EntityManager.GetAspect<TransformAspect>(currentEntity).worldPosition=newPos;
            }      
        }

        public void OnDestroy(ref SystemState state)
        {
            _entities.Dispose();
        }

        [BurstDiscard]
        private void DebugInfo(string param)
        {
            UnityEngine.Debug.Log(param);
        }
    }
}
