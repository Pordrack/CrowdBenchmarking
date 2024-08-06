using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

//Less optimized, but allows for managed data types, used here for educational purpose
public partial class CubeSpawnerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<CubeSpawner>();
    }

    protected override void OnUpdate()
    {
        //Ensure that the logic will only play once
        this.Enabled = false; 

        var cubeSpawner=SystemAPI.GetSingleton<CubeSpawner>();

        for(int i=0; i<cubeSpawner.NbrToSpawn; i++)
        {
            Entity spawnedEntity=EntityManager.Instantiate(cubeSpawner.CubePrefabEntity);
            float3 randomPos = new float3(UnityEngine.Random.Range(-5, 5), 0,UnityEngine.Random.Range(-5, 5));
            EntityManager.SetComponentData(spawnedEntity, LocalTransform.FromPosition(randomPos));
            //If the component is already present, it gets replaced
            //SystemAPI.SetComponent has better performances than EntityManager.SetComponentData
            SystemAPI.SetComponent(spawnedEntity, new Movement { MovementVector = Vector3.forward });
        }

        //To spawn huge number, use the other instantiate methods that uses native arrays as second parameter for example
        //Also, the System works by having huge arrays of entities. As such, modifying the entities you're querying will fuck up things and cause lags, so if you're
        //querying/iterating with a job through entities with X components, and you want to add new entities with this component, you'll need to use 
        //var ecb=new EntityCommandBuffer(Unity.Collections.Allocator.Temp); for[...]{ ecb.Instantiate([...] ecb.SetComponent[...]} ecb.Playback(EntityManager);
        //this way no structural changes during the foreach
        //Adding components is another structural change, so it needs to be sparse
        //WorldUpdateAllocator is a special allocator working for 1-2 frame
    }
}
