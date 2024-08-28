using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Transforms;

namespace CrowdNPC
{
    public partial struct PhysicsNPCSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsNPC>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            foreach ((var physicsNPC,var transformAspect, Entity entity) in SystemAPI.Query<RefRO<PhysicsNPC>, Latios.Transforms.TransformAspect>().WithEntityAccess())
            {
                var localTransform = physicsNPC.ValueRO.PhysicObjectLT;
                if (physicsNPC.ValueRO.LocalTransformStored)
                { 
                }
                localTransform=state.EntityManager.GetComponentData<LocalTransform>(physicsNPC.ValueRO.PhysicObjectEntity);
                transformAspect.worldPosition = localTransform.Position;
                UnityEngine.Debug.Log("Position: " + transformAspect.worldPosition);
            }
        }
    }   
}
