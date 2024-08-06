using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CubeSpawnerAuthoring : MonoBehaviour
{
    public GameObject CubePrefab;
    public int NbrToSpawn;

    public class Baker : Baker<CubeSpawnerAuthoring>
    {
        public override void Bake(CubeSpawnerAuthoring authoring)
        {
            Entity entity=GetEntity(TransformUsageFlags.None);

            AddComponent(entity, new CubeSpawner
            {
                CubePrefabEntity = GetEntity(authoring.CubePrefab, TransformUsageFlags.Dynamic),
                NbrToSpawn = authoring.NbrToSpawn
            });
        }
    }
}


public partial struct CubeSpawner : IComponentData
{
    public Entity CubePrefabEntity;
    public int NbrToSpawn;
}