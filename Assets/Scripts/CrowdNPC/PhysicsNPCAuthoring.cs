using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class PhysicsNPCAuthoring : MonoBehaviour
{
    public GameObject PhysicObject;

    public class Baker : Baker<PhysicsNPCAuthoring>
    {
        public override void Bake(PhysicsNPCAuthoring authoring)
        {
            var entity = GetEntity(authoring.gameObject, TransformUsageFlags.Dynamic);
            var physicsObjectEntity = GetEntity(authoring.PhysicObject.gameObject, TransformUsageFlags.Dynamic);
            AddComponent(entity, new PhysicsNPC()
            {
                PhysicObjectEntity = physicsObjectEntity,
                LocalTransformStored=false,
            });
        }
    }
}

public partial struct PhysicsNPC : IComponentData
{
    public LocalTransform PhysicObjectLT;
    public Entity PhysicObjectEntity;
    public bool LocalTransformStored;
}
