using Latios.Transforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CrowdNPC
{
    public partial struct MixAndMathNPCSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MixAndMatchNPC>();
        }

        public void OnUpdate(ref SystemState state)
        {
            state.Enabled = false;
            foreach ((var mixAndMatchNpc, Entity entity) in SystemAPI.Query<MixAndMatchNPC>().WithEntityAccess())
            {
                UnityEngine.Debug.Log("Boop !");
                state.EntityManager.SetComponentEnabled<MixAndMatchNPC>(entity,false);
            }
        }
    }
}
