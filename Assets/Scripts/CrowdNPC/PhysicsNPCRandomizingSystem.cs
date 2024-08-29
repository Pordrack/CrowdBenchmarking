using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CrowdNPC.Kinemation
{
    public partial struct PhysicsNPCRandomizingSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicNPC>();
            state.RequireForUpdate<PhysicNPCRandomConstraints>();
        }

        public static float2 GetRandomDirection()
        {
            float angle = UnityEngine.Random.Range(0, 360);
            return new float2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach ((var physicNPC, var physicNPCRandomConstraints, Entity entity) in SystemAPI.Query<RefRW<PhysicNPC>, RefRO<PhysicNPCRandomConstraints>>().WithEntityAccess())
            {
                physicNPC.ValueRW.Weight = physicNPCRandomConstraints.ValueRO.WeightCurve.Evaluate(UnityEngine.Random.value);
                physicNPC.ValueRW.Velocity=physicNPCRandomConstraints.ValueRO.VelocityAmplitudeCurve.Evaluate(UnityEngine.Random.value)*GetRandomDirection();
                state.EntityManager.SetComponentEnabled<PhysicNPCRandomConstraints>(entity, false);
            }    
        }
    }
}
